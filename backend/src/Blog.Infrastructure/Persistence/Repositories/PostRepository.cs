using Microsoft.EntityFrameworkCore;
using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;

namespace Blog.Infrastructure.Persistence.Repositories;

public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;

    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Posts.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await _context.Posts.FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);

    public async Task<List<Post>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Posts.ToListAsync(cancellationToken);

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default) =>
        await _context.Posts.AddAsync(post, cancellationToken);

    public void Update(Post post) =>
        _context.Posts.Update(post);

    public void Delete(Post post) =>
        _context.Posts.Remove(post);

    public async Task<bool> HasAssociatedPostsAsync(Guid authorId, CancellationToken cancellationToken = default) =>
        await _context.Posts.AnyAsync(p => p.AuthorId == authorId, cancellationToken);
}
