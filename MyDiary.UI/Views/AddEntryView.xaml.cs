using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MyDiary.Services.Diary;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class AddEntryView : UserControl
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
    private readonly DateOnly _createDate;
    private readonly ObservableCollection<ActivitySelectionItem> _activities = new();

    public AddEntryView(object? parameter)
    {
        InitializeComponent();

        _editing = parameter as EntryPreview;
        _createDate = parameter is DateOnly d ? d : DateOnly.FromDateTime(System.DateTime.Today);
        HeaderText.Text = _editing is null ? $"Создать запись • {_createDate:dd.MM.yyyy}" : "Изменить запись";
        
        // Отладка: выведем в консоль, какую дату получили
        System.Diagnostics.Debug.WriteLine($"AddEntryView received date: {_createDate:dd.MM.yyyy}, parameter type: {parameter?.GetType().Name ?? "null"}");

        if (ActivitiesList is not null)
        {
            ActivitiesList.ItemsSource = _activities;
        }

        if (_editing is not null)
        {
            TitleBox.Text = _editing.Title;
            ContentBox.Text = _editing.Content;
            SetMood(_editing.MoodLevel);
        }

        Loaded += async (_, _) =>
        {
            await LoadActivitiesAsync();
            RestoreSelectedActivities();
            SyncActivitiesEmptyState();
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
            _activities.Add(new ActivitySelectionItem(a));
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

        var userId = UiServices.CurrentUser.Id;
        var date = _editing?.Date ?? _createDate;
        
        // Отладка: выведем дату, которая будет передана в CreateAsync
        System.Diagnostics.Debug.WriteLine($"ApplyAsync: _createDate={_createDate:dd.MM.yyyy}, final date={date:dd.MM.yyyy}, _editing={_editing?.Title ?? "null"}");
        
        var title = string.IsNullOrWhiteSpace(TitleBox?.Text) ? GetSelectedMoodName() : TitleBox.Text;
        var content = ContentBox?.Text ?? string.Empty;
        var moodLevel = GetSelectedMoodLevel();
        var activities = _activities.Where(a => a.IsSelected).Select(a => a.Name).ToList();

        if (_editing is null)
        {
            await DiaryEntryAppService.CreateAsync(
                UiServices.DiaryEntryRepository,
                UiServices.UserActivityRepository,
                userId,
                date,
                title,
                content,
                moodLevel,
                activities);
        }
        else
        {
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
        }

        UiServices.Navigation.Navigate(AppPage.Entries);
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

    private string GetSelectedMoodName()
    {
        if (Mood1.IsChecked == true) return "Плохо";
        if (Mood2.IsChecked == true) return "Ниже нормы";
        if (Mood3.IsChecked == true) return "Нормально";
        if (Mood4.IsChecked == true) return "Хорошо";
        if (Mood5.IsChecked == true) return "Отлично";

        return "😐";
    }
    
}
