using System.Windows.Controls;
using System.Windows;
using MyDiary.Services.Users;
using MyDiary.UI.Navigation;
using MyDiary.UI.Security;

namespace MyDiary.UI.Views;

public partial class RegisterView : UserControl
{
    private bool _passwordVisible = true;
    private bool _repeatPasswordVisible = true;
    private bool _updatingPassword;
    private bool _updatingRepeatPassword;

    public RegisterView()
    {
        InitializeComponent();
        ErrorMessageText.Text = " ";
        ErrorMessageText.Margin = new Thickness(0, 0, 0, 0);

        UpdatePasswordVisibility();
        UpdateRepeatPasswordVisibility();
    }

    private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _passwordVisible = !_passwordVisible;
        UpdatePasswordVisibility();
    }

    private void ToggleRepeatPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        _repeatPasswordVisible = !_repeatPasswordVisible;
        UpdateRepeatPasswordVisibility();
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

    private void UpdateRepeatPasswordVisibility()
    {
        if (RepeatPasswordBox is null || RepeatPasswordTextBox is null)
        {
            return;
        }

        if (_repeatPasswordVisible)
        {
            RepeatPasswordTextBox.Text = RepeatPasswordBox.Password;
            RepeatPasswordTextBox.Visibility = Visibility.Visible;
            RepeatPasswordBox.Visibility = Visibility.Collapsed;
        }
        else
        {
            RepeatPasswordBox.Password = RepeatPasswordTextBox.Text;
            RepeatPasswordBox.Visibility = Visibility.Visible;
            RepeatPasswordTextBox.Visibility = Visibility.Collapsed;
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
            if (_passwordVisible)
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
            if (_passwordVisible)
            {
                PasswordBox.Password = PasswordTextBox.Text;
            }
        }
        finally
        {
            _updatingPassword = false;
        }
    }

    private void RepeatPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_updatingRepeatPassword)
        {
            return;
        }

        try
        {
            _updatingRepeatPassword = true;
            if (_repeatPasswordVisible)
            {
                RepeatPasswordTextBox.Text = RepeatPasswordBox.Password;
            }
        }
        finally
        {
            _updatingRepeatPassword = false;
        }
    }

    private void RepeatPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingRepeatPassword)
        {
            return;
        }

        try
        {
            _updatingRepeatPassword = true;
            if (_repeatPasswordVisible)
            {
                RepeatPasswordBox.Password = RepeatPasswordTextBox.Text;
            }
        }
        finally
        {
            _updatingRepeatPassword = false;
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        var login = (LoginTextBox.Text ?? string.Empty).Trim();
        var name = (NameTextBox.Text ?? string.Empty).Trim();
        var password = PasswordBox.Password;
        var repeatPassword = RepeatPasswordBox.Password;

        var result = await UserRegistrationService.RegisterAsync(
            UiServices.UserRepository,
            login,
            name,
            password,
            repeatPassword);

        if (!result.IsSuccess || result.User is null)
        {
            ErrorMessageText.Text = result.Error;
            ErrorMessageText.Margin = new Thickness(0, 142, 0, 9);
            return;
        }

        UiServices.CurrentUser = result.User;
        UiServices.Navigation.Navigate(AppPage.Subscription);
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Login);
    }
}
