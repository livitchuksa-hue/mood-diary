using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.InMemory.Repositories;

public class InMemoryDiaryEntryRepository : IDiaryEntryRepository
{
    public Task<IReadOnlyList<DiaryEntry>> GetByUserAndPeriodAsync(Guid userId, DateOnly? start, DateOnly? end, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<DiaryEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(DiaryEntry entry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(DiaryEntry entry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
