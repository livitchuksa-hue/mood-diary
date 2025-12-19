using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;
using MyDiary.Services.Subscriptions;

namespace MyDiary.Services.Payments;

public sealed record PaySubscriptionResult(bool IsSuccess, string Error);

public static class PaymentAppService
{
    public static async Task<PaySubscriptionResult> PaySubscriptionAsync(
        IPaymentMethodRepository paymentMethodRepository,
        ISubscriptionRepository subscriptionRepository,
        Guid userId,
        SubscriptionPlan plan,
        string cardNumber,
        int expMonth,
        int expYear2,
        string cvc,
        DateTime? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        var now = nowUtc ?? DateTime.UtcNow;

        if (!FakePaymentProvider.ValidateCardInput(cardNumber, expMonth, expYear2, cvc, out var validateError))
        {
            return new PaySubscriptionResult(false, validateError);
        }

        if (!FakePaymentProvider.CanCardExist(cardNumber, expMonth, expYear2, cvc, out var canExistError))
        {
            return new PaySubscriptionResult(false, canExistError);
        }

        if (!FakePaymentProvider.TryCreateToken(cardNumber, expMonth, expYear2, cvc, out var pm, out var tokenError))
        {
            return new PaySubscriptionResult(false, tokenError);
        }

        var amountRub = SubscriptionRenewalService.GetPriceRub(plan);
        if (!FakePaymentProvider.TryChargeByToken(pm.Token, amountRub, out var chargeError))
        {
            return new PaySubscriptionResult(false, chargeError);
        }

        // store payment method in DB (make default)
        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = "Fake",
            Token = pm.Token,
            Last4 = pm.Last4,
            Brand = pm.Brand,
            CreatedAtUtc = now,
            IsDefault = true
        };

        await paymentMethodRepository.AddAsync(paymentMethod, cancellationToken);
        await paymentMethodRepository.SetDefaultAsync(userId, paymentMethod.Id, cancellationToken);

        // upsert subscription
        var latest = await subscriptionRepository.GetLatestByUserIdAsync(userId, cancellationToken);
        var months = (int)plan;

        if (latest is null)
        {
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Plan = plan,
                PaidAtUtc = now,
                ExpiresAtUtc = now.AddMonths(months)
            };

            await subscriptionRepository.AddAsync(subscription, cancellationToken);
        }
        else
        {
            var baseDate = latest.ExpiresAtUtc > now ? latest.ExpiresAtUtc : now;
            latest.Plan = plan;
            latest.PaidAtUtc = now;
            latest.ExpiresAtUtc = baseDate.AddMonths(months);
            await subscriptionRepository.UpdateAsync(latest, cancellationToken);
        }

        return new PaySubscriptionResult(true, string.Empty);
    }
}
