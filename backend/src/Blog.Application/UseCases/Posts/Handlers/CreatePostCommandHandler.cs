using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Commands;
using Blog.Domain.Entities;
using Blog.Shared;

namespace Blog.Application.UseCases.Posts.Handlers;

public class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, Guid>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(CreatePostCommand command, CancellationToken cancellationToken = default)
    {
        var existingPost = await _postRepository.GetBySlugAsync(command.Slug, cancellationToken);
        if (existingPost != null)
        {
            return Result.Failure<Guid>(new Error("Post.SlugNotUnique", "A post with this slug already exists."));
        }

        var postResult = Post.Create(
            command.Title,
            command.Slug,
            command.Summary,
            command.Content,
            command.Tags,
            command.CoverImage,
            command.AuthorId
        );

        if (postResult.IsFailure)
        {
            return Result.Failure<Guid>(postResult.Error);
        }

        await _postRepository.AddAsync(postResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(postResult.Value.Id);
    }
}
