using MyDiary.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MyDiary.Services.Activities;
using MyDiary.UI.Models;
using MyDiary.UI.Properties;
using MyDiary.UI.Security;
using MyDiary.UI.Themes;
using MyDiary.Domain.Entities;

namespace MyDiary.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        Loaded += async (_, _) =>
        {
            SyncThemeCombo();
            await RenderActivitiesAsync();
        };
        UserName.Text= UiServices.CurrentUser.Name;

    }

    private void SyncThemeCombo()
    {
        if (ThemeCombo is null)
        {
            return;
        }

        var theme = Settings.Default.Theme;
        ThemeCombo.SelectedIndex = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }

    private async Task RenderActivitiesAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        var userId = UiServices.CurrentUser.Id;
        var activities = await UserActivityAppService.GetByUserIdAsync(UiServices.UserActivityRepository, userId);
        var models = activities
            .Select(x => new ActivityEditModel(x.Id, x.Name, x.Description))
            .ToList();

        if (ActivitiesList is null)
        {
            return;
        }

        ActivitiesList.ItemsSource = models;

        if (ActivitiesEmptyStatePanel is not null)
        {
            ActivitiesEmptyStatePanel.Visibility = models.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ApplyThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (ThemeCombo?.SelectedIndex == 1)
        {
            Settings.Default.Theme = "dark";
            Settings.Default.Save();
            InfoMessageText.Text="Тема будет применена после перезапуска приложения";
            InfoMessageText.Margin = new Thickness(0, 9, 0, 0);
            return;
        }

        Settings.Default.Theme = "light";
        Settings.Default.Save();
        InfoMessageText.Text = "Тема будет применена после перезапуска приложения";
        InfoMessageText.Margin = new Thickness(0, 9, 0, 0);
    }

    private void ActivityCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ActivityEditModel activity })
        {
            return;
        }

        UiServices.Navigation.Navigate(AppPage.EditActivity, activity);
    }
    private void CreateActivityButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.CreateActivity);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.CurrentUser = null;
        RememberMeStorage.Clear();
        UiServices.Navigation.Navigate(AppPage.Login);
    }
}
