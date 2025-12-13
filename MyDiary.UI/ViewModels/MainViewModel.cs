using System.Windows;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private AppPage _currentPage;

    public MainViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.Navigated += OnNavigated;

        NavigateEntriesCommand = new RelayCommand(() => _navigation.Navigate(AppPage.Entries));
        NavigateStatisticsCommand = new RelayCommand(() => _navigation.Navigate(AppPage.Statistics));
        NavigateSettingsCommand = new RelayCommand(() => _navigation.Navigate(AppPage.Settings));
    }

    public RelayCommand NavigateEntriesCommand { get; }
    public RelayCommand NavigateStatisticsCommand { get; }
    public RelayCommand NavigateSettingsCommand { get; }

    public AppPage CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (!SetProperty(ref _currentPage, value))
            {
                return;
            }

            OnPropertyChanged(nameof(EntriesButtonStyle));
            OnPropertyChanged(nameof(StatisticsButtonStyle));
            OnPropertyChanged(nameof(SettingsButtonStyle));
        }
    }

    public Style EntriesButtonStyle => ResolveTopBarStyle(IsEntriesSection(CurrentPage));
    public Style StatisticsButtonStyle => ResolveTopBarStyle(CurrentPage == AppPage.Statistics);
    public Style SettingsButtonStyle => ResolveTopBarStyle(IsSettingsSection(CurrentPage));

    private void OnNavigated(AppPage page, object? parameter)
    {
        CurrentPage = page;
    }

    private static bool IsEntriesSection(AppPage page)
    {
        return page is AppPage.Entries or AppPage.AddEntry or AppPage.EntryDetails;
    }

    private static bool IsSettingsSection(AppPage page)
    {
        return page is AppPage.Settings or AppPage.CreateActivity or AppPage.EditActivity;
    }

    private static Style ResolveTopBarStyle(bool isActive)
    {
        var key = isActive ? "PrimaryButton" : "GhostButton";
        return (Style)Application.Current.Resources[key];
    }
}
