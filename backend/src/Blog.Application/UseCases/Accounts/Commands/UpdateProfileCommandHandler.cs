using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Commands;

public class UpdateProfileCommandHandler : ICommandHandler<UpdateProfileCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure(new Error("User.NotFound", "The user was not found."));
        }

        var updateResult = user.UpdateProfile(command.DisplayName, command.Bio, command.AvatarUrl);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
