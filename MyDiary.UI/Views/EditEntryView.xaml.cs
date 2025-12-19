using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using MyDiary.Services.Diary;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EditEntryView : UserControl
{
    private sealed class ActivitySelectionItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public ActivitySelectionItem(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private readonly EntryPreview? _editing;
    private readonly ObservableCollection<ActivitySelectionItem> _activities = new();

    public EditEntryView(object? parameter)
    {
        InitializeComponent();

        _editing = parameter as EntryPreview;

        if (_editing is not null)
        {
            TitleBox.Text = _editing.Title;
            ContentBox.Text = _editing.Content;
            SetMood(_editing.MoodLevel);
        }

        if (ActivitiesList is not null)
        {
            ActivitiesList.ItemsSource = _activities;
        }

        Loaded += async (_, _) =>
        {
            await LoadActivitiesAsync();
            RestoreSelectedActivities();
            SyncActivitiesEmptyState();
            SyncDeleteButton();
        };
    }

    private async System.Threading.Tasks.Task LoadActivitiesAsync()
    {
        _activities.Clear();

        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        var names = await DiaryEntryAppService.GetAllActivityNamesAsync(
            UiServices.UserActivityRepository,
            UiServices.CurrentUser.Id);

        foreach (var a in names)
        {
            var item = new ActivitySelectionItem(a);
            item.PropertyChanged += (_, _) => SyncDeleteButton();
            _activities.Add(item);
        }
    }

    private void RestoreSelectedActivities()
    {
        if (_editing?.Activities is null)
        {
            return;
        }

        var selected = _editing.Activities
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var item in _activities)
        {
            item.IsSelected = selected.Contains(item.Name);
        }
    }

    private void SyncActivitiesEmptyState()
    {
        if (ActivitiesEmptyStatePanel is null)
        {
            return;
        }

        ActivitiesEmptyStatePanel.Visibility = _activities.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SyncDeleteButton()
    {
        if (DeleteActivitiesButton is null)
        {
            return;
        }

        DeleteActivitiesButton.Visibility = _activities.Any(a => a.IsSelected) ? Visibility.Visible : Visibility.Collapsed;
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

    private int GetSelectedMoodLevel()
    {
        if (Mood1.IsChecked == true) return 1;
        if (Mood2.IsChecked == true) return 2;
        if (Mood3.IsChecked == true) return 3;
        if (Mood4.IsChecked == true) return 4;
        if (Mood5.IsChecked == true) return 5;

        return 3;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.EntryDetails, _editing);
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

        if (_editing is null)
        {
            UiServices.Navigation.Navigate(AppPage.Entries);
            return;
        }

        var userId = UiServices.CurrentUser.Id;
        var date = _editing.Date;
        var title = TitleBox?.Text ?? string.Empty;
        var content = ContentBox?.Text ?? string.Empty;
        var moodLevel = GetSelectedMoodLevel();
        var activities = _activities.Where(a => a.IsSelected).Select(a => a.Name).ToList();

        await DiaryEntryAppService.UpdateAsync(
            UiServices.DiaryEntryRepository,
            UiServices.UserActivityRepository,
            userId,
            _editing.Id,
            date,
            title,
            content,
            moodLevel,
            activities);

        var updatedDto = await DiaryEntryAppService.GetPreviewByIdAsync(UiServices.DiaryEntryRepository, _editing.Id);
        if (updatedDto is null)
        {
            UiServices.Navigation.Navigate(AppPage.Entries);
            return;
        }

        var updated = new EntryPreview(
            Id: updatedDto.Id,
            Date: updatedDto.Date,
            Title: updatedDto.Title,
            Summary: updatedDto.Summary,
            Content: updatedDto.Content,
            MoodLevel: updatedDto.MoodStatus,
            Mood: DiaryEntryAppService.MoodEmoji(updatedDto.MoodStatus),
            CreatedAt: updatedDto.CreatedAtUtc.ToLocalTime(),
            Activities: updatedDto.Activities);

        UiServices.Navigation.Navigate(AppPage.EntryDetails, updated);
    }

    private void DeleteActivitiesButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _activities)
        {
            if (item.IsSelected)
            {
                item.IsSelected = false;
            }
        }

        SyncDeleteButton();
    }
}
