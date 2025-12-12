using System.Windows.Controls;
using System.Windows;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class LoginView : UserControl
{
    private bool _passwordVisible;
    private bool _updatingPassword;

    public LoginView()
    {
        InitializeComponent();
    }

    private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        UpdatePasswordVisibility();
    }

    private void UpdatePasswordVisibility()
    {
        if (PasswordBox is null || PasswordTextBox is null)
        {
            return;
        }

        if (_passwordVisible)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }
        else
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordTextBox.Visibility = Visibility.Collapsed;
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_updatingPassword)
        {
            return;
        }

        try
        {
            _updatingPassword = true;
            if (_passwordVisible && PasswordTextBox is not null)
            {
                PasswordTextBox.Text = PasswordBox.Password;
            }
        }
        finally
        {
            _updatingPassword = false;
        }
    }

    private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingPassword)
        {
            return;
        }

        try
        {
            _updatingPassword = true;
            if (_passwordVisible && PasswordBox is not null)
            {
                PasswordBox.Password = PasswordTextBox.Text;
            }
        }
        finally
        {
            _updatingPassword = false;
        }
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
