using MyDiary.Domain.Entities;

namespace MyDiary.Data.Interfaces.Repositories;

public interface IDiaryEntryRepository
{
    Task<IReadOnlyList<DiaryEntry>> GetByUserAndPeriodAsync(Guid userId, DateOnly? start, DateOnly? end, CancellationToken cancellationToken = default);
    Task<DiaryEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DiaryEntry?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(DiaryEntry entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(DiaryEntry entry, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
