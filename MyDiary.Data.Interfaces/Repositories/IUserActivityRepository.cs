using MyDiary.Domain.Entities;

namespace MyDiary.Data.Interfaces.Repositories;

public interface IUserActivityRepository
{
    Task<IReadOnlyList<UserActivity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserActivity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(UserActivity activity, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserActivity activity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}