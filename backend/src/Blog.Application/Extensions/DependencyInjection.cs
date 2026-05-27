using Microsoft.Extensions.DependencyInjection;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Application.UseCases.Posts.Queries;

namespace Blog.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Commands
        services.AddScoped<ICommandHandler<CreatePostCommand, Guid>, CreatePostCommandHandler>();
        services.AddScoped<ICommandHandler<ChangePostAuthorCommand>, ChangePostAuthorCommandHandler>();
        services.AddScoped<ICommandHandler<LoginCommand, TokenResponse>, LoginCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenCommand, TokenResponse>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateProfileCommand>, UpdateProfileCommandHandler>();
        services.AddScoped<ICommandHandler<RegisterCommand, Guid>, RegisterCommandHandler>();
        services.AddScoped<ICommandHandler<AssignRoleCommand>, AssignRoleCommandHandler>();
        services.AddScoped<ICommandHandler<InactivateAccountCommand>, InactivateAccountCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteAccountCommand>, DeleteAccountCommandHandler>();
        services.AddScoped<ICommandHandler<UpdatePasswordCommand>, UpdatePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<EditPostCommand>, EditPostCommandHandler>();
        services.AddScoped<ICommandHandler<DeletePostCommand>, DeletePostCommandHandler>();
        services.AddScoped<ICommandHandler<InactivatePostCommand>, InactivatePostCommandHandler>();

        // Queries
        services.AddScoped<IQueryHandler<GetPostBySlugQuery, PostResponse>, GetPostBySlugQueryHandler>();
        services.AddScoped<IQueryHandler<GetPostsQuery, PagedResponse<PostResponse>>, GetPostsQueryHandler>();

        return services;
    }
}
