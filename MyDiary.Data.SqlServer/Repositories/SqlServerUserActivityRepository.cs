using Microsoft.EntityFrameworkCore;
using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.SqlServer.Repositories;

public sealed class SqlServerUserActivityRepository : IUserActivityRepository
{
    private readonly MyDiaryDbContext _context;

    public SqlServerUserActivityRepository(MyDiaryDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserActivity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.UserActivities
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<UserActivity?> GetByUserIdAndNameAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        name = (name ?? string.Empty).Trim();
        return _context.UserActivities
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public Task<UserActivity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.UserActivities
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(UserActivity activity, CancellationToken cancellationToken = default)
    {
        _context.UserActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserActivity activity, CancellationToken cancellationToken = default)
    {
        _context.UserActivities.Update(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserActivities
            .Include(x => x.DiaryEntryActivities)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return;
        }

        _context.UserActivities.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
