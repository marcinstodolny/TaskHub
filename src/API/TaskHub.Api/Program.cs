using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
            builder.Services.Configure<DemoAuthOptions>(builder.Configuration.GetSection(DemoAuthOptions.SectionName));
            builder.Services.AddSingleton<JwtTokenGenerator>();

            var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
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
    }
}
