using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Accounts.Commands;

public record UpdateProfileCommand(Guid UserId, string DisplayName, string Bio, string AvatarUrl) : ICommand;
