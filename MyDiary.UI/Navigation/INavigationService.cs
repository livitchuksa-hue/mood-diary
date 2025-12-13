using System;

namespace MyDiary.UI.Navigation;

public interface INavigationService
{
    event Action<AppPage, object?>? Navigated;
    void Navigate(AppPage page, object? parameter = null);
}
