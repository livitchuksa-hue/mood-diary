namespace MyDiary.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public DateTime PaidAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
