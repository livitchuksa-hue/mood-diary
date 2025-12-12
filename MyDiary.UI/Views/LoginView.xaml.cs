using System.Windows.Controls;
using System.Windows;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Subscription);
    }

    private void GoToRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Register);
    }
}
