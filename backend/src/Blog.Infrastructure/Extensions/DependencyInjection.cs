using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Blog.Application.Common.Interfaces;
using Blog.Infrastructure.Identity;
using Blog.Infrastructure.Persistence;
using Blog.Infrastructure.Persistence.Repositories;

namespace Blog.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext & MySQL
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<BlogDbContext>(options =>
            options.UseMySQL(connectionString));

        services.AddIdentityCore<Account>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<BlogDbContext>();

        // Repositories & Shared Interfaces
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
