using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;
using MyDiary.UI.Navigation;

namespace MyDiary.UI;

public static class UiServices
{
    public static INavigationService Navigation { get; set; } = null!;
    public static IUserRepository UserRepository { get; set; } = null!;
    public static User? CurrentUser { get; set; }
}