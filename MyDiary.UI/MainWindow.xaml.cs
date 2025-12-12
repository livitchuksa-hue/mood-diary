using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyDiary.UI.Navigation;
using MyDiary.UI.Views;

namespace MyDiary.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        UiServices.Navigation = new MyDiary.UI.Navigation.NavigationService(NavigateInternal);
        UiServices.Navigation.Navigate(AppPage.Login);
    }

    private void NavigateInternal(AppPage page, object? parameter)
    {
        TopBar.Visibility = page is AppPage.Statistics or AppPage.Entries or AppPage.AddEntry or AppPage.Settings or AppPage.EntryDetails
            ? Visibility.Visible
            : Visibility.Collapsed;

        MainContent.Content = page switch
        {
            AppPage.Login => new LoginView(),
            AppPage.Register => new RegisterView(),
            AppPage.Subscription => new SubscriptionView(),

            AppPage.Statistics => new StatisticsView(),
            AppPage.Entries => new EntriesView(),
            AppPage.AddEntry => new AddEntryView(),
            AppPage.Settings => new SettingsView(),

            AppPage.EntryDetails => new EntryDetailsView(parameter),
            _ => new LoginView()
        };
    }
}