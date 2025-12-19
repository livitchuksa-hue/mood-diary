using System.Windows;
using System.Windows.Controls;
using MyDiary.Services.Activities;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EditActivityView : UserControl
{
    private readonly ActivityEditModel? _activity;

    public EditActivityView(object? parameter)
    {
        InitializeComponent();

        _activity = parameter as ActivityEditModel;

        if (_activity is not null)
        {
            TitleBox.Text = _activity.Title;
            DescriptionBox.Text = _activity.Description;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ApplyAsync();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        _ = DeleteAsync();
    }

    private async System.Threading.Tasks.Task ApplyAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        if (_activity is null || _activity.Id == Guid.Empty)
        {
            UiServices.Navigation.Navigate(AppPage.Settings);
            return;
        }

        var name = (TitleBox?.Text ?? string.Empty).Trim();
        var description = DescriptionBox?.Text ?? string.Empty;

        if (name.Length == 0)
        {
            MessageBox.Show("Название активности не заполнено");
            return;
        }

        await UserActivityAppService.UpdateAsync(
            UiServices.UserActivityRepository,
            UiServices.CurrentUser.Id,
            _activity.Id,
            name,
            description);

        UiServices.Navigation.Navigate(AppPage.Settings);
    }

    private async System.Threading.Tasks.Task DeleteAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        if (_activity is null || _activity.Id == Guid.Empty)
        {
            UiServices.Navigation.Navigate(AppPage.Settings);
            return;
        }

        await UserActivityAppService.DeleteAsync(
            UiServices.UserActivityRepository,
            UiServices.CurrentUser.Id,
            _activity.Id);

        UiServices.Navigation.Navigate(AppPage.Settings);
    }
}
