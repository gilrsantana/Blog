using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;

namespace Blog.Application.UseCases.Accounts.Commands;

public record LoginCommand(string Email, string Password) : ICommand<TokenResponse>;
