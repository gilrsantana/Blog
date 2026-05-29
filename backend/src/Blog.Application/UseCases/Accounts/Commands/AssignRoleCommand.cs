using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Accounts.Commands;

public record AssignRoleCommand(Guid AccountId, string RoleName) : ICommand;
