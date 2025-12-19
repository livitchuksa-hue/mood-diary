using Microsoft.EntityFrameworkCore;
using MyDiary.Data.SqlServer;
using MyDiary.Domain.Entities;
using MyDiary.Services.Payments;

namespace MyDiary.Services.Subscriptions;

public static class SubscriptionRenewalService
{
    public static async Task<bool> TryAutoRenewAsync(MyDiaryDbContext db, Guid userId, DateTime? nowUtc = null)
    {
        var now = nowUtc ?? DateTime.UtcNow;

        var subscription = await db.Subscriptions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ExpiresAtUtc)
            .FirstOrDefaultAsync();

        if (subscription is null)
        {
            return false;
        }

        if (subscription.ExpiresAtUtc > now)
        {
            return false;
        }

        var paymentMethod = await db.PaymentMethods
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (paymentMethod is null)
        {
            return false;
        }

        var amountRub = GetPriceRub(subscription.Plan);
        if (!FakePaymentProvider.TryChargeByToken(paymentMethod.Token, amountRub, out _))
        {
            return false;
        }

        var months = (int)subscription.Plan;
        subscription.PaidAtUtc = now;
        subscription.ExpiresAtUtc = now.AddMonths(months);
        db.Subscriptions.Update(subscription);
        await db.SaveChangesAsync();
        return true;
    }

    public static int GetPriceRub(SubscriptionPlan plan)
    {
        return plan switch
        {
            SubscriptionPlan.Monthly => 320,
            SubscriptionPlan.HalfEar => 1728,
            SubscriptionPlan.Yearly => 3072,
            _ => 320
        };
    }
}
