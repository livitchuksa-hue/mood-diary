using MyDiary.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MyDiary.UI.Models;
using MyDiary.UI.Themes;

namespace MyDiary.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            SyncLanguageHint();
            SyncThemeCombo();
            RenderActivities();
        };
    }

    private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SyncLanguageHint();
    }

    private void SyncLanguageHint()
    {
        if (ActivityHint is null)
        {
            return;
        }

        if (LanguageCombo?.SelectedItem is ComboBoxItem item)
        {
            ActivityHint.Text = $"Выбран язык: {item.Content}";
        }
    }

    private void SyncThemeCombo()
    {
        if (ThemeCombo is null)
        {
            return;
        }

        var theme = ThemeManager.DetectCurrentTheme();
        ThemeCombo.SelectedIndex = theme == AppTheme.Dark ? 1 : 0;
    }

    private static IReadOnlyList<ActivityEditModel> BuildDemoActivities()
    {
        return new List<ActivityEditModel>
        {
        };
    }

    private void RenderActivities()
    {
        var activities = BuildDemoActivities();

        if (ActivitiesList is null)
        {
            return;
        }

        ActivitiesList.ItemsSource = activities;

        if (ActivitiesEmptyStatePanel is not null)
        {
            ActivitiesEmptyStatePanel.Visibility = activities.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ApplyThemeButton_Click(object sender, RoutedEventArgs e)
    {
        if (ThemeCombo?.SelectedIndex == 1)
        {
            ThemeManager.ApplyTheme(AppTheme.Dark);
            return;
        }

        ThemeManager.ApplyTheme(AppTheme.Light);
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
        UiServices.Navigation.Navigate(AppPage.Login);
    }
}
