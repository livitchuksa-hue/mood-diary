using Microsoft.EntityFrameworkCore;
using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Data.SqlServer.Repositories;

public sealed class SqlServerPaymentMethodRepository : IPaymentMethodRepository
{
    private readonly MyDiaryDbContext _context;

    public SqlServerPaymentMethodRepository(MyDiaryDbContext context)
    {
        _context = context;
    }

    public Task<PaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.PaymentMethods
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.PaymentMethods
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default)
    {
        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDefaultAsync(Guid userId, Guid paymentMethodId, CancellationToken cancellationToken = default)
    {
        var methods = await _context.PaymentMethods
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var m in methods)
        {
            m.IsDefault = m.Id == paymentMethodId;
        }

        if (methods.Count > 0)
        {
            _context.PaymentMethods.UpdateRange(methods);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
