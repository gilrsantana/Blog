using Blog.Application.Extensions;
using Blog.Infrastructure.Extensions;
using Blog.Presentation.Middleware;
using Scalar.AspNetCore;

namespace Blog.Presentation.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi();

        services
            .AddApplication()
            .AddInfrastructure(configuration);

        return services;
    }

    public static void Configure(this WebApplication app)
    {

        app.UseMiddleware<CustomExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
                {
                    options.WithTitle("Blog Web API")
                        .WithTheme(ScalarTheme.Moon)
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}
