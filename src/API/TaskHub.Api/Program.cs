using Microsoft.OpenApi;
using TaskHub.Api.Filters;
using TaskHub.Api.Hubs;
using TaskHub.Application;
using TaskHub.Infrastructure;
using TaskHub.Infrastructure.Hosting;

namespace TaskHub.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
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
        builder.Services.AddScoped<RequireAuthSchemaAvailableFilter>();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.ApplyInfrastructureInitialization();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHub<TaskActivityHub>("/hubs/task-activity");

        app.Run();
    }
}
