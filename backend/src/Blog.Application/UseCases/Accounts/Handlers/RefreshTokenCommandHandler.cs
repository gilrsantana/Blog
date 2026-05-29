using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Handlers;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<TokenResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.AccessToken) || string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidToken", "Tokens are required."));
        }

        return await _identityService.RefreshTokenAsync(command.AccessToken, command.RefreshToken);
    }
}
