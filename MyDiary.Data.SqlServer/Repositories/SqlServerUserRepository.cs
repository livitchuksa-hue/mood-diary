using Microsoft.EntityFrameworkCore;
using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.SqlServer.Repositories;

public class SqlServerUserRepository : IUserRepository
{
    private readonly MyDiaryDbContext _context;

    public SqlServerUserRepository(MyDiaryDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        => _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Login == login, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}