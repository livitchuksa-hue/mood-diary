using Microsoft.EntityFrameworkCore;
using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.SqlServer.Repositories;

public sealed class SqlServerDiaryEntryRepository : IDiaryEntryRepository
{
    private readonly MyDiaryDbContext _context;

    public SqlServerDiaryEntryRepository(MyDiaryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DiaryEntry>> GetByUserAndPeriodAsync(
        Guid userId,
        DateOnly? start,
        DateOnly? end,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DiaryEntries
            .AsNoTracking()
            .Include(x => x.DiaryEntryActivities)
            .ThenInclude(x => x.UserActivity)
            .Where(x => x.UserId == userId);

        if (start.HasValue)
        {
            query = query.Where(x => x.Date >= start.Value);
        }

        if (end.HasValue)
        {
            query = query.Where(x => x.Date <= end.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<DiaryEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.DiaryEntries
            .AsNoTracking()
            .Include(x => x.DiaryEntryActivities)
            .ThenInclude(x => x.UserActivity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<DiaryEntry?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.DiaryEntries
            .Include(x => x.DiaryEntryActivities)
            .ThenInclude(x => x.UserActivity)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(DiaryEntry entry, CancellationToken cancellationToken = default)
    {
        _context.DiaryEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DiaryEntry entry, CancellationToken cancellationToken = default)
    {
        _context.DiaryEntries.Update(entry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DiaryEntries
            .Include(x => x.DiaryEntryActivities)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return;
        }

        _context.DiaryEntries.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
