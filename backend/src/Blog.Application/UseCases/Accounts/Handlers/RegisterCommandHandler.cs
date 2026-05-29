using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Accounts.Commands;
using Blog.Domain.Entities;
using Blog.Shared;

namespace Blog.Application.UseCases.Accounts.Handlers;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, Guid>
{
    private readonly IIdentityService _identityService;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return Result.Failure<Guid>(new Error("User.InvalidEmail", "Email is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Result.Failure<Guid>(new Error("Auth.PasswordRequired", "Password is required."));
        }

        var existingUser = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<Guid>(new Error("User.EmailNotUnique", "A user with this email already exists."));
        }

        var accountId = Guid.NewGuid();

        var identityResult = await _identityService.RegisterAsync(accountId, command.Email, command.Password);
        if (identityResult.IsFailure)
        {
            return Result.Failure<Guid>(identityResult.Error);
        }

        var userResult = User.Create(
            accountId,
            command.Email,
            command.DisplayName,
            command.Bio,
            command.AvatarUrl
        );

        if (userResult.IsFailure)
        {
            return Result.Failure<Guid>(userResult.Error);
        }

        await _userRepository.AddAsync(userResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(accountId);
    }
}
