using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Handlers;

public class InactivateAccountCommandHandler : ICommandHandler<InactivateAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InactivateAccountCommandHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(InactivateAccountCommand command, CancellationToken cancellationToken = default)
    {
        var identityResult = await _identityService.InactivateAccountAsync(command.AccountId);
        if (identityResult.IsFailure)
        {
            return identityResult;
        }

        var user = await _userRepository.GetByIdAsync(command.AccountId, cancellationToken);
        if (user != null)
        {
            user.Inactivate();
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
