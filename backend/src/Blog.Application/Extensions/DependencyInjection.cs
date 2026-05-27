using Microsoft.Extensions.DependencyInjection;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Application.UseCases.Posts.Commands;

namespace Blog.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreatePostCommand, Guid>, CreatePostCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePostAuthorCommand>, ChangePostAuthorCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, TokenResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, TokenResponse>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProfileCommand>, UpdateProfileCommandHandler>();

        return services;
    }
}
