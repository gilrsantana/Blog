using Blog.Domain.Entities;

namespace Blog.Application.Common.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Post post, CancellationToken cancellationToken = default);
    void Update(Post post);
    void Delete(Post post);
    Task<bool> HasAssociatedPostsAsync(Guid authorId, CancellationToken cancellationToken = default);
}
