using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Posts.Handlers;

public class ChangePostAuthorCommandHandler : ICommandHandler<ChangePostAuthorCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePostAuthorCommandHandler(
        IPostRepository postRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(ChangePostAuthorCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(command.PostId, cancellationToken);
        if (post == null)
        {
            return Result.Failure(new Error("Post.NotFound", "The post was not found."));
        }

        var newAuthor = await _userRepository.GetByIdAsync(command.NewAuthorId, cancellationToken);
        if (newAuthor == null)
        {
            return Result.Failure(new Error("User.NotFound", "The new author was not found."));
        }

        if (!newAuthor.IsActive)
        {
            return Result.Failure(new Error("User.Inactive", "The new author account is inactive."));
        }

        var changeResult = post.ChangeAuthor(command.NewAuthorId);
        if (changeResult.IsFailure)
        {
            return changeResult;
        }

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
