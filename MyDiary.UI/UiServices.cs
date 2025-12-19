using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Data.SqlServer;
using MyDiary.Domain.Entities;
using MyDiary.UI.Navigation;

namespace MyDiary.UI;

public static class UiServices
{
    public static INavigationService Navigation { get; set; } = null!;
    public static IUserRepository UserRepository { get; set; } = null!;
    public static ISubscriptionRepository SubscriptionRepository { get; set; } = null!;
    public static IPaymentMethodRepository PaymentMethodRepository { get; set; } = null!;
    public static IDiaryEntryRepository DiaryEntryRepository { get; set; } = null!;
    public static IUserActivityRepository UserActivityRepository { get; set; } = null!;
    public static MyDiaryDbContext DbContext { get; set; } = null!;
    public static User? CurrentUser { get; set; }
}