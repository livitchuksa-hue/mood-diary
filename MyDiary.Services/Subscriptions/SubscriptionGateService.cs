using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;
using MyDiary.Services.Payments;

namespace MyDiary.Services.Subscriptions;

public enum SubscriptionGateResultType
{
    Allowed = 1,
    NeedPayment = 2
}

public sealed record SubscriptionGateResult(
    SubscriptionGateResultType Type,
    Subscription? Subscription,
    string Error);

public static class SubscriptionGateService
{
    public static async Task<SubscriptionGateResult> EnsureAccessAsync(
        ISubscriptionRepository subscriptionRepository,
        IPaymentMethodRepository paymentMethodRepository,
        Guid userId,
        DateTime? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        var now = nowUtc ?? DateTime.UtcNow;

        var active = await subscriptionRepository.GetActiveByUserIdAsync(userId, now, cancellationToken);
        if (active is not null)
        {
            return new SubscriptionGateResult(SubscriptionGateResultType.Allowed, active, string.Empty);
        }

        var latest = await subscriptionRepository.GetLatestByUserIdAsync(userId, cancellationToken);
        if (latest is null)
        {
            return new SubscriptionGateResult(SubscriptionGateResultType.NeedPayment, null, "Подписка не найдена");
        }

        // subscription exists, but expired => attempt auto-renew via default payment method
        var paymentMethod = await paymentMethodRepository.GetDefaultByUserIdAsync(userId, cancellationToken);
        if (paymentMethod is null)
        {
            return new SubscriptionGateResult(SubscriptionGateResultType.NeedPayment, latest, "Нет способа оплаты для автопродления");
        }

        var amountRub = SubscriptionRenewalService.GetPriceRub(latest.Plan);
        if (!FakePaymentProvider.TryChargeByToken(paymentMethod.Token, amountRub, out var chargeError))
        {
            return new SubscriptionGateResult(SubscriptionGateResultType.NeedPayment, latest, chargeError);
        }

        var months = (int)latest.Plan;
        latest.PaidAtUtc = now;
        latest.ExpiresAtUtc = now.AddMonths(months);
        await subscriptionRepository.UpdateAsync(latest, cancellationToken);

        return new SubscriptionGateResult(SubscriptionGateResultType.Allowed, latest, string.Empty);
    }
}
