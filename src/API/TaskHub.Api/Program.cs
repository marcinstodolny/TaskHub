using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using TaskHub.Api.Auth;
using TaskHub.Api.Auth.Options;
using TaskHub.Application;
using TaskHub.Application.abstraction;
using TaskHub.Infrastructure;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Api
{
    public class Program
    {
        private const string AddUsersTableMigrationId = "20260406163148_AddUsersTable";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                var bearerScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste your JWT token."
                };

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TaskHub API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", bearerScheme);

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document, null),
                        new List<string>()
                    }
                });
            });
            builder.Services.AddSignalR();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
            builder.Services.AddSingleton<IUserPasswordHasher, AspNetUserPasswordHasher>();
            builder.Services.AddScoped<DemoUserInitializer>();
            builder.Services.AddScoped<IAuthSchemaAvailabilityChecker, AuthSchemaAvailabilityChecker>();
            builder.Services.AddScoped<DatabaseUserAuthenticator>();

            builder.Services
                .AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is not configured.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is not configured.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "JWT signing key is not configured.")
                .ValidateOnStart();

            var demoAuthOptionsBuilder = builder.Services
                .AddOptions<DemoAuthOptions>()
                .BindConfiguration(DemoAuthOptions.SectionName);

            var demoAuthSupportedEnvironment = builder.Environment.IsDevelopment()
                || builder.Environment.IsEnvironment("Testing");

            // Demo auth is intentionally a local/demo-only path. It assumes fresh-start usage
            // with a seeded demo user and does not support legacy JWT compatibility.
            demoAuthOptionsBuilder
                .Validate(options => !options.Enabled || demoAuthSupportedEnvironment,
                    "Demo auth is supported only in Development and Testing environments.")
                .Validate(options => !options.Enabled || options.UserId != Guid.Empty, "Demo auth user id is not configured.")
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Username), "Demo auth username is not configured.")
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Password), "Demo auth password is not configured.")
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.DisplayName), "Demo auth display name is not configured.")
                .ValidateOnStart();

            builder.Services.AddSingleton<JwtTokenGenerator>();

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();

            builder.Services
                .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
                {
                    var jwtOptions = jwtOptionsAccessor.Value;
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = signingKey,
                        ClockSkew = TimeSpan.Zero,
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbMigrations");
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();
                var migrator = dbContext.GetService<IMigrator>();
                var demoUserInitializer = scope.ServiceProvider.GetRequiredService<DemoUserInitializer>();

                const int maxAttempts = 10;
                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        logger.LogInformation("Applying EF Core migrations (attempt {Attempt}/{MaxAttempts})...", attempt, maxAttempts);

                        var appliedMigrations = dbContext.Database.GetAppliedMigrations().ToHashSet(StringComparer.OrdinalIgnoreCase);
                        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (!appliedMigrations.Contains(AddUsersTableMigrationId)
                            && pendingMigrations.Contains(AddUsersTableMigrationId))
                        {
                            migrator.Migrate(AddUsersTableMigrationId);
                            logger.LogInformation("Migration {MigrationId} applied.", AddUsersTableMigrationId);
                        }

                        if (dbContext.Database.GetAppliedMigrations().Contains(AddUsersTableMigrationId, StringComparer.OrdinalIgnoreCase))
                        {
                            demoUserInitializer.EnsureDemoUserAsync().GetAwaiter().GetResult();
                        }

                        dbContext.Database.Migrate();
                        logger.LogInformation("Migrations applied.");
                        break;
                    }
                    catch (Exception exception)
                    {
                        logger.LogWarning(exception, "Migration attempt {Attempt} failed.", attempt);

                        if (attempt == maxAttempts)
                            throw;

                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
