using System.Windows.Controls;
using System.Windows;
using MyDiary.Services.Security;
using MyDiary.Services.Subscriptions;
using MyDiary.UI.Security;
using MyDiary.UI.Navigation;
using System.Windows.Media;

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

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var login = LoginTextBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ElementGrid.Children.Remove(ErrorText);
            ErrorText.Margin = new Thickness(0, 18, 0, 0);
            ErrorText.Text = "Введите логин и пароль";
            ElementGrid.Children.Add(ErrorText);
            return;
        }

        var user = await UiServices.UserRepository.GetByLoginAsync(login);
        if (user is null)
        {
            ElementGrid.Children.Remove(ErrorText);
            ErrorText.Margin = new Thickness(0, 18, 0, 0);
            ErrorText.Text = "Неверный логин или пароль";
            ElementGrid.Children.Add(ErrorText);

            return;
        }

        var ok = PasswordHasher.Verify(password, user.PasswordHash);
        if (!ok)
        {
            ElementGrid.Children.Remove(ErrorText);
            ErrorText.Margin = new Thickness(0, 18, 0, 0);
            ErrorText.Text = "Неверный логин или пароль";
            ElementGrid.Children.Add(ErrorText);

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

        if (RememberMeCheckBox?.IsChecked == true)
        {
            RememberMeStorage.SaveUserId(user.Id);
        }
        else
        {
            RememberMeStorage.Clear();
        }
        ElementGrid.Children.Remove(ErrorText);

        if (gate.Type == SubscriptionGateResultType.Allowed)
        {
            UiServices.Navigation.Navigate(AppPage.Entries);
        }
        else
        {
            UiServices.Navigation.Navigate(AppPage.Subscription);
        }
    }

    private void GoToRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Register);
    }
}
