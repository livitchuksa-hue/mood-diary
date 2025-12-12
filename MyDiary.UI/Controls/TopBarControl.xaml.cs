using System.Windows.Controls;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Controls;

public partial class TopBarControl : UserControl
{
    public TopBarControl()
    {
        InitializeComponent();
    }

    private void StatisticsButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Statistics);
    }

    private void EntriesButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private void SettingsButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }
}
