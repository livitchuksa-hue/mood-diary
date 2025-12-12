using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.InMemory.Repositories;

public class InMemorySubscriptionRepository : ISubscriptionRepository
{
    public Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
