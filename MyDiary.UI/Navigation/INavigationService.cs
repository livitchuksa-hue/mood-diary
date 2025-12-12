namespace MyDiary.UI.Navigation;

public interface INavigationService
{
    void Navigate(AppPage page, object? parameter = null);
}
