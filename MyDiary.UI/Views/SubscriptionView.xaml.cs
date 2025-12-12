using System.Windows;
using System.Windows.Controls;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class SubscriptionView : UserControl
{
    public SubscriptionView()
    {
        InitializeComponent();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Calendar);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Login);
    }
}
