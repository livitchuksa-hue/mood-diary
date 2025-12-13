using MyDiary.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MyDiary.UI.Models;

namespace MyDiary.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            SyncLanguageHint();
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

    private static IReadOnlyList<ActivityEditModel> BuildDemoActivities()
    {
        return new List<ActivityEditModel>
        {
            new("Спорт", "Тренировка, пробежка, прогулка."),
            new("Музыка", "Слушал музыку или играл на инструменте."),
            new("Спорт", "Тренировка, пробежка, прогулка."),
            new("Музыка", "Слушал музыку или играл на инструменте."),
            new("Спорт", "Тренировка, пробежка, прогулка."),
            new("Музыка", "Слушал музыку или играл на инструменте."),
            new("Спорт", "Тренировка, пробежка, прогулка."),
            new("Музыка", "Слушал музыку или играл на инструменте."),
            new("Спорт", "Тренировка, пробежка, прогулка."),
            new("Музыка", "Слушал музыку или играл на инструменте.")
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
