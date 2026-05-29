using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Shared;

namespace Blog.Application.UseCases.Posts.Handlers;

public class EditPostCommandHandler : ICommandHandler<EditPostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditPostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(EditPostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(command.Id, cancellationToken);
        if (post == null)
        {
            return Result.Failure(new Error("Post.NotFound", "Post not found."));
        }

        if (post.Slug != command.Slug)
        {
            var existingPost = await _postRepository.GetBySlugAsync(command.Slug, cancellationToken);
            if (existingPost != null)
            {
                return Result.Failure(new Error("Post.SlugNotUnique", "A post with this slug already exists."));
            }
        }

        var updateResult = post.Update(
            command.Title,
            command.Slug,
            command.Summary,
            command.Content,
            command.Tags,
            command.CoverImage
        );

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
