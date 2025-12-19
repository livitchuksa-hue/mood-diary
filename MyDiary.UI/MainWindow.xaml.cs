using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyDiary.Services.Payments;
using MyDiary.Services.Subscriptions;
using MyDiary.UI.Navigation;
using MyDiary.UI.Security;
using MyDiary.UI.ViewModels;
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

        Loaded += MainWindow_Loaded;

        UiServices.Navigation = new MyDiary.UI.Navigation.NavigationService(NavigateInternal);

        var vm = new MainViewModel(UiServices.Navigation);
        DataContext = vm;
        TopBar.DataContext = vm;

        UiServices.Navigation.Navigate(AppPage.Login);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!RememberMeStorage.TryGetRememberedUserId(out var userId))
        {
            return;
        }

        try
        {
            var user = await UiServices.UserRepository.GetByIdAsync(userId);
            if (user is null)
            {
                RememberMeStorage.Clear();
                return;
            }

            UiServices.CurrentUser = user;

            SubscriptionGateResult gate;
            try
            {
                gate = await SubscriptionGateService.EnsureAccessAsync(
                    UiServices.SubscriptionRepository,
                    UiServices.PaymentMethodRepository,
                    user.Id);
            }
            catch
            {
                gate = new SubscriptionGateResult(SubscriptionGateResultType.NeedPayment, null, "Ошибка проверки подписки");
            }

            UiServices.Navigation.Navigate(gate.Type == SubscriptionGateResultType.Allowed
                ? AppPage.Entries
                : AppPage.Subscription);
        }
        catch
        {
            RememberMeStorage.Clear();
        }
    }

    private void NavigateInternal(AppPage page, object? parameter)
    {
        TopBar.Visibility = page is AppPage.Statistics or AppPage.Entries or AppPage.AddEntry or AppPage.EditEntry or AppPage.Settings or AppPage.CreateActivity or AppPage.EditActivity or AppPage.EntryDetails or AppPage.DayEntries
            ? Visibility.Visible
            : Visibility.Collapsed;

        MainContent.Content = page switch
        {
            AppPage.Login => new LoginView(),
            AppPage.Register => new RegisterView(),
            AppPage.Subscription => new SubscriptionView(),
            AppPage.Payment => new PaymentView(parameter),

            AppPage.Statistics => new StatisticsView(),
            AppPage.Entries => new EntriesView(),
            AppPage.AddEntry => new AddEntryView(parameter),
            AppPage.EditEntry => new EditEntryView(parameter),
            AppPage.Settings => new SettingsView(),

            AppPage.CreateActivity => new CreateActivityView(),
            AppPage.EditActivity => new EditActivityView(parameter),

            AppPage.EntryDetails => new EntryDetailsView(parameter),
            _ => new LoginView()
        };
    }
}