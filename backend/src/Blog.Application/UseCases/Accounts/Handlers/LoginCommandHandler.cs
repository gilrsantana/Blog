using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Handlers;

public class LoginCommandHandler : ICommandHandler<LoginCommand, TokenResponse>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<TokenResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return Result.Failure<TokenResponse>(new Error("Auth.InvalidCredentials", "Email and password are required."));
        }

        return await _identityService.LoginAsync(command.Email, command.Password);
    }
}
