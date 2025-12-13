using MyDiary.UI.Navigation;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyDiary.UI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();

        Loaded += (_, _) => SyncLanguageHint();
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

    private void RenderActivities()
    {
        var Activities = 0;
        var rows = 1;
        for (int i = 0; i <= Activities; i++)
        {
            if (i % 10 == 0) rows++;
        }
        ActivityGrid.Rows = rows;
        for (int i = 0; i <= Activities; i++) 
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = (Brush)Application.Current.Resources["Brush.Surface2"],
                BorderBrush = cellBorder,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4)
            };
        }
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
