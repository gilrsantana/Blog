namespace Blog.Application.UseCases.Posts.Queries;

public record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
