using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Handlers;

public class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
{
    private readonly IIdentityService _identityService;

    public AssignRoleCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> HandleAsync(AssignRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RoleName))
        {
            return Result.Failure(new Error("Auth.RoleNameRequired", "Role name is required."));
        }

        return await _identityService.AssignRoleAsync(command.AccountId, command.RoleName);
    }
}
