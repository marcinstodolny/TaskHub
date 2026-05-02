using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskHub.Application.Abstractions;
using TaskHub.Application.Abstractions.Repositories.Command;
using TaskHub.Application.Abstractions.Repositories.Query;
using TaskHub.Infrastructure.Authentication.AccessTokens;
using TaskHub.Infrastructure.Authentication.Bootstrap;
using TaskHub.Infrastructure.Authentication.CurrentUser;
using TaskHub.Infrastructure.Authentication.Options;
using TaskHub.Infrastructure.Authentication.Passwords;
using TaskHub.Infrastructure.Authentication.Schema;
using TaskHub.Infrastructure.Persistence;
using TaskHub.Infrastructure.Repositories.Command;
using TaskHub.Infrastructure.Repositories.Query;
using TaskHub.Infrastructure.Time;

namespace TaskHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("TaskHub")
            ?? throw new InvalidOperationException("Missing connection string 'TaskHub'.");

        services.AddDbContext<TaskHubDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<ITaskItemQueryRepository, TaskItemQueryRepository>();
        services.AddScoped<ITaskActivityQueryRepository, TaskActivityQueryRepository>();
        services.AddScoped<IDashboardQueryRepository, DashboardQueryRepository>();
        services.AddScoped<ITaskItemCommandRepository, TaskItemCommandRepository>();
        services.AddScoped<ITaskActivityCommandRepository, TaskActivityCommandRepository>();
        services.AddScoped<ITaskListQueryRepository, TaskListQueryRepository>();
        services.AddScoped<ITaskListCommandRepository, TaskListCommandRepository>();
        services.AddScoped<IUserAuthenticationQueryRepository, UserAuthenticationQueryRepository>();
        services.AddScoped<IUserCommandRepository, UserCommandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddSingleton<IUserPasswordHasher, AspNetUserPasswordHasher>();
        services.AddSingleton<IAccessTokenGenerator, JwtAccessTokenGenerator>();
        services.AddScoped<IAuthSchemaAvailabilityChecker, DatabaseAuthSchemaAvailabilityChecker>();
        services.AddScoped<DemoUserInitializer>();
        services.AddScoped<DemoDataInitializer>();

        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is not configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is not configured.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey), "JWT signing key is not configured.")
            .ValidateOnStart();

        var demoAuthOptionsBuilder = services
            .AddOptions<DemoAuthOptions>()
            .BindConfiguration(DemoAuthOptions.SectionName);

        var demoAuthSupportedEnvironment = environment.IsDevelopment()
            || environment.IsEnvironment("Testing");

        demoAuthOptionsBuilder
            .Validate(options => !options.Enabled || demoAuthSupportedEnvironment,
                "Demo auth is supported only in Development and Testing environments.")
            .Validate(options => !options.Enabled || options.UserId != Guid.Empty, "Demo auth user id is not configured.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Username), "Demo auth username is not configured.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Password), "Demo auth password is not configured.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.DisplayName), "Demo auth display name is not configured.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
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

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
