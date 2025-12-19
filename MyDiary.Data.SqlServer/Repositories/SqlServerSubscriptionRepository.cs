using Microsoft.EntityFrameworkCore;
using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.SqlServer.Repositories;

public sealed class SqlServerSubscriptionRepository : ISubscriptionRepository
{
    private readonly MyDiaryDbContext _context;

    public SqlServerSubscriptionRepository(MyDiaryDbContext context)
    {
        _context = context;
    }

    public Task<Subscription?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.Subscriptions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ExpiresAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<Subscription?> GetActiveByUserIdAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default)
        => _context.Subscriptions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ExpiresAtUtc > nowUtc)
            .OrderByDescending(x => x.ExpiresAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
