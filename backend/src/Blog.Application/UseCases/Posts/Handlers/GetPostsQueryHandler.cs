using Blog.Application.Common.CQRS;
using Blog.Application.Common.Interfaces;
using Blog.Application.UseCases.Posts.Queries;
using Blog.Shared;

namespace Blog.Application.UseCases.Posts.Handlers;

public class GetPostsQueryHandler : IQueryHandler<GetPostsQuery, PagedResponse<PostResponse>>
{
    private readonly IPostRepository _postRepository;

    public GetPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<PagedResponse<PostResponse>>> HandleAsync(GetPostsQuery query, CancellationToken cancellationToken = default)
    {
        var allPosts = await _postRepository.GetAllAsync(cancellationToken);
        
        var queryable = allPosts.AsQueryable();

        if (!query.IsAdmin)
        {
            queryable = queryable.Where(p => p.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            queryable = queryable.Where(p => p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(t => t.Trim())
                                                  .Contains(query.Tag.Trim(), StringComparer.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            queryable = queryable.Where(p => p.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                             p.Content.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                             p.Summary.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var sorted = queryable.OrderByDescending(p => p.CreatedAt).ToList();

        int totalCount = sorted.Count;
        int page = Math.Max(1, query.Page);
        int pageSize = Math.Max(1, query.PageSize);
        var items = sorted.Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .Select(post => new PostResponse(
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
                          ))
                          .ToList();

        var response = new PagedResponse<PostResponse>(items, totalCount, page, pageSize);
        return Result.Success(response);
    }
}
