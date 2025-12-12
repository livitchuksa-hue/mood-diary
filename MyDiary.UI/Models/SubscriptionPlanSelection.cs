namespace MyDiary.UI.Models;

public enum SubscriptionPlanKind
{
    Month = 1,
    HalfYear = 2,
    Year = 3
}

public sealed class SubscriptionPlanSelection
{
    public SubscriptionPlanKind Kind { get; }
    public string Title { get; }
    public int PriceRub { get; }

    public SubscriptionPlanSelection(SubscriptionPlanKind kind, string title, int priceRub)
    {
        Kind = kind;
        Title = title;
        PriceRub = priceRub;
    }

    public string PriceText => $"{PriceRub} ₽";

    public static SubscriptionPlanSelection For(SubscriptionPlanKind kind)
    {
        return kind switch
        {
            SubscriptionPlanKind.Month => new SubscriptionPlanSelection(kind, "Месяц", 320),
            SubscriptionPlanKind.HalfYear => new SubscriptionPlanSelection(kind, "6 месяцев", 1728),
            SubscriptionPlanKind.Year => new SubscriptionPlanSelection(kind, "Год", 3072),
            _ => new SubscriptionPlanSelection(SubscriptionPlanKind.Month, "Месяц", 320)
        };
    }
}
