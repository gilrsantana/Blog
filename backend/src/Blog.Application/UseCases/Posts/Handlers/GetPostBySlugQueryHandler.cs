using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Queries;
using Blog.Shared;

namespace Blog.Application.UseCases.Posts.Handlers;

public class GetPostBySlugQueryHandler : IQueryHandler<GetPostBySlugQuery, PostResponse>
{
    private readonly IPostRepository _postRepository;

    public GetPostBySlugQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PostResponse>> HandleAsync(GetPostBySlugQuery query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.Slug))
        {
            return Result.Failure<PostResponse>(new Error("Post.SlugRequired", "Slug is required."));
        }

        var post = await _postRepository.GetBySlugAsync(query.Slug, cancellationToken);
        if (post == null)
        {
            return Result.Failure<PostResponse>(new Error("Post.NotFound", "Post not found."));
        }

        if (!post.IsPublished && !query.IsAdmin)
        {
            return Result.Failure<PostResponse>(new Error("Post.Unpublished", "Access to unpublished post requires administrator privileges."));
        }

        var response = new PostResponse(
            post.Id,
            post.Title,
            post.Slug,
            post.Summary,
            post.Content,
            post.Tags,
            post.CoverImage,
            post.ReadingTime,
            post.IsPublished,
            post.CreatedAt,
            post.AuthorId
        );

        return Result.Success(response);
    }
}
