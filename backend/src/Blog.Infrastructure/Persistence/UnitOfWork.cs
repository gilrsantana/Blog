using Blog.Application.Common.Interfaces;

namespace Blog.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly BlogDbContext _context;

    public UnitOfWork(BlogDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
