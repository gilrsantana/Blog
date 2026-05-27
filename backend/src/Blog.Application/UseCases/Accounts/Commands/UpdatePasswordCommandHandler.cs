using System.Threading;
using System.Threading.Tasks;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Commands;

public class UpdatePasswordCommandHandler : ICommandHandler<UpdatePasswordCommand>
{
    private readonly IIdentityService _identityService;

    public UpdatePasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> HandleAsync(UpdatePasswordCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentPassword) || string.IsNullOrWhiteSpace(command.NewPassword))
        {
            return Result.Failure(new Error("Auth.PasswordRequired", "Current password and new password are required."));
        }

        return await _identityService.UpdatePasswordAsync(command.AccountId, command.CurrentPassword, command.NewPassword);
    }
}
