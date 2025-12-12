using System.Windows;
using System.Windows.Controls;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class AddEntryView : UserControl
{
    private readonly EntryPreview? _editing;

    public AddEntryView(object? parameter)
    {
        InitializeComponent();

        _editing = parameter as EntryPreview;
        HeaderText.Text = _editing is null ? "Создать запись" : "Изменить запись";

        if (_editing is not null)
        {
            TitleBox.Text = _editing.Title;
            ContentBox.Text = _editing.Summary;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private string GetSelectedMoodEmoji()
    {
        if (Mood1.IsChecked == true) return "😔";
        if (Mood2.IsChecked == true) return "😣";
        if (Mood3.IsChecked == true) return "😐";
        if (Mood4.IsChecked == true) return "🙂";
        if (Mood5.IsChecked == true) return "😊";

        return "😐";
    }
}
