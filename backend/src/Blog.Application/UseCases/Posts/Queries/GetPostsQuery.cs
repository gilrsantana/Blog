using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Queries;

public record GetPostsQuery(
    string? Tag = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10,
    bool IsAdmin = false) : IQuery<PagedResponse<PostResponse>>;
