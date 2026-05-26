using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;

namespace Blog.Application.UseCases.Accounts.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : ICommand<TokenResponse>;
