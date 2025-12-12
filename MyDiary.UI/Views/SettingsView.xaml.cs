using System;
using System.Windows;
using System.Windows.Controls;
using MyDiary.UI.Navigation;

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

    private void CreateActivityButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.CreateActivity);
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Login);
    }
}
