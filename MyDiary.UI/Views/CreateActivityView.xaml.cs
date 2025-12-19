using System.Windows;
using System.Windows.Controls;
using MyDiary.Services.Activities;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class CreateActivityView : UserControl
{
    public CreateActivityView()
    {
        InitializeComponent();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Settings);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        _ = ApplyAsync();
    }

    private async System.Threading.Tasks.Task ApplyAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        var name = TitleBox?.Text ?? string.Empty;
        var description = DescriptionBox?.Text ?? string.Empty;

        name = name.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show("Название активности не заполнено");
            return;
        }

        await UserActivityAppService.CreateAsync(
            UiServices.UserActivityRepository,
            UiServices.CurrentUser.Id,
            name,
            description);

        UiServices.Navigation.Navigate(AppPage.Settings);
    }
}
