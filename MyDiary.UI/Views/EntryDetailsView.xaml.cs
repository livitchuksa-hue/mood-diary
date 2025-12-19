using System;
using System.Windows;
using System.Windows.Controls;
using MyDiary.Services.Diary;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EntryDetailsView : UserControl
{
    private readonly EntryPreview? _entry;

    public EntryDetailsView(object? parameter)
    {
        InitializeComponent();

        _entry = parameter as EntryPreview;

        TitleText.Text = _entry?.Title ?? "Запись";
        MetaText.Text = _entry is null ? "" : $"{_entry.Mood} • {_entry.CreatedAt:dd.MM.yyyy HH:mm}";

        if (TitleBox is not null)
        {
            TitleBox.Text = _entry?.Title ?? "";
        }

        if (ContentBox is not null)
        {
            ContentBox.Text = _entry?.Content ?? "";
        }

        SetMood(_entry?.MoodLevel ?? 0);

        if (ActivitiesList is not null)
        {
            ActivitiesList.ItemsSource = _entry?.Activities ?? Array.Empty<string>();
        }

        if (ActivitiesEmptyStatePanel is not null)
        {
            ActivitiesEmptyStatePanel.Visibility = _entry?.Activities is null || _entry.Activities.Length == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private void SetMood(int moodLevel)
    {
        Mood1.IsChecked = moodLevel == 1;
        Mood2.IsChecked = moodLevel == 2;
        Mood3.IsChecked = moodLevel == 3;
        Mood4.IsChecked = moodLevel == 4;
        Mood5.IsChecked = moodLevel == 5;

        if (moodLevel < 1 || moodLevel > 5)
        {
            Mood3.IsChecked = true;
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.EditEntry, _entry);
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        _ = DeleteAsync();
    }

    private async System.Threading.Tasks.Task DeleteAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        if (_entry is not null)
        {
            await DiaryEntryAppService.DeleteAsync(
                UiServices.DiaryEntryRepository,
                UiServices.CurrentUser.Id,
                _entry.Id);
        }

        UiServices.Navigation.Navigate(AppPage.Entries);
    }

    private void ActivityCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string activity } || string.IsNullOrWhiteSpace(activity))
        {
            return;
        }

        _ = OpenActivityAsync(activity);
    }

    private async System.Threading.Tasks.Task OpenActivityAsync(string activityName)
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        var found = await UiServices.UserActivityRepository.GetByUserIdAndNameAsync(
            UiServices.CurrentUser.Id,
            activityName);

        if (found is null)
        {
            MessageBox.Show("Активность не найдена в базе данных");
            return;
        }

        UiServices.Navigation.Navigate(AppPage.EditActivity, new ActivityEditModel(found.Id, found.Name, found.Description));
    }
}
