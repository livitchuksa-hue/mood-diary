using MyDiary.Domain.Entities;

namespace MyDiary.Data.Interfaces.Repositories;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetDefaultByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentMethod paymentMethod, CancellationToken cancellationToken = default);
    Task SetDefaultAsync(Guid userId, Guid paymentMethodId, CancellationToken cancellationToken = default);
}
