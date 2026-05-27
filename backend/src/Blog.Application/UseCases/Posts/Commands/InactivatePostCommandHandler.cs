using System.Threading;
using System.Threading.Tasks;
using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Shared;

namespace Blog.Application.UseCases.Posts.Commands;

public class InactivatePostCommandHandler : ICommandHandler<InactivatePostCommand>
{
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InactivatePostCommandHandler(IPostRepository postRepository, IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(InactivatePostCommand command, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(command.Id, cancellationToken);
        if (post == null)
        {
            return Result.Failure(new Error("Post.NotFound", "Post not found."));
        }

        post.Inactivate();
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
