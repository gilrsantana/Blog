using Blog.Application.Common.CQRS;

namespace Blog.Application.UseCases.Posts.Queries;

public record GetPostBySlugQuery(string Slug, bool IsAdmin) : IQuery<PostResponse>;
