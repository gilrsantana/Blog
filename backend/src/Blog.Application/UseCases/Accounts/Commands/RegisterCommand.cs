using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Accounts.Commands;

public record RegisterCommand(
    string Email, 
    string Password, 
    string DisplayName, 
    string Bio, 
    string AvatarUrl) : ICommand<Guid>;
