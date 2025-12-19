using MyDiary.Domain.Entities;

namespace MyDiary.Data.Interfaces.Repositories;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetLatestByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveByUserIdAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default);
}
