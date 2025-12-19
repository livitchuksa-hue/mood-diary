namespace MyDiary.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public required string Provider { get; set; }
    public required string Token { get; set; }

    public required string Last4 { get; set; }
    public required string Brand { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public bool IsDefault { get; set; }
}
