using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using TaskHub.Api.Auth;
using TaskHub.Api.Auth.Options;
using TaskHub.Application;
using TaskHub.Infrastructure;
using TaskHub.Infrastructure.Persistence;

namespace TaskHub.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

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

            if (IsDemoAuthEnabled(builder.Environment))
            {
                demoAuthOptionsBuilder
                    .Validate(options => !string.IsNullOrWhiteSpace(options.Username), "Demo auth username is not configured.")
                    .Validate(options => !string.IsNullOrWhiteSpace(options.Password), "Demo auth password is not configured.")
                    .ValidateOnStart();
            }

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

            if (app.Environment.IsDevelopment())
            {
                using var scope = app.Services.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbMigrations");
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();

                const int maxAttempts = 10;
                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        logger.LogInformation("Applying EF Core migrations (attempt {Attempt}/{MaxAttempts})...", attempt, maxAttempts);
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

        private static bool IsDemoAuthEnabled(IHostEnvironment environment) =>
            environment.IsDevelopment() || environment.IsEnvironment("Testing");
    }
}
