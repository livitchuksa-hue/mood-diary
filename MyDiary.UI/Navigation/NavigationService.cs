namespace MyDiary.UI.Navigation;

public class NavigationService : INavigationService
{
    private readonly Action<AppPage, object?> _navigate;

    public event Action<AppPage, object?>? Navigated;

    public NavigationService(Action<AppPage, object?> navigate)
    {
        _navigate = navigate;
    }

    public void Navigate(AppPage page, object? parameter = null)
    {
        _navigate(page, parameter);
        Navigated?.Invoke(page, parameter);
    }
}
