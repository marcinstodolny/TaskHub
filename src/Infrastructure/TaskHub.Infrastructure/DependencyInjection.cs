using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskHub.Application.abstraction;
using TaskHub.Application.abstraction.Repository.Command;
using TaskHub.Application.abstraction.Repository.Query;
using TaskHub.Infrastructure.Persistence;
using TaskHub.Infrastructure.Repositories.Command;
using TaskHub.Infrastructure.Repositories.Query;
using TaskHub.Infrastructure.Time;

namespace TaskHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("TaskHub") ?? throw new InvalidOperationException("Missing connection string 'TaskHub'.");

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
        services.AddScoped<ITaskItemCommandRepository, TaskItemCommandRepository>();
        services.AddScoped<ITaskListQueryRepository, TaskListQueryRepository>();
        services.AddScoped<ITaskListCommandRepository, TaskListCommandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}