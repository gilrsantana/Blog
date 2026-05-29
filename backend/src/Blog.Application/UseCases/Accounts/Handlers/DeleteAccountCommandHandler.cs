using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Handlers;

public class DeleteAccountCommandHandler : ICommandHandler<DeleteAccountCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IPostRepository postRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(DeleteAccountCommand command, CancellationToken cancellationToken = default)
    {
        var hasPosts = await _postRepository.HasAssociatedPostsAsync(command.AccountId, cancellationToken);
        if (hasPosts)
        {
            return Result.Failure(new Error("Auth.CannotDeleteAccountWithPosts", "Account deletion is blocked because posts are associated with this user."));
        }

        var identityResult = await _identityService.DeleteAccountAsync(command.AccountId);
        if (identityResult.IsFailure)
        {
            return identityResult;
        }

        var user = await _userRepository.GetByIdAsync(command.AccountId, cancellationToken);
        if (user != null)
        {
            _userRepository.Delete(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
