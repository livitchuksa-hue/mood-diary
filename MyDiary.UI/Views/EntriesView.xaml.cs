using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyDiary.UI;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EntriesView : UserControl
{
    private DateTime _monthCursor = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private readonly ObservableCollection<EntryPreview> _items = new();
    private readonly DateTime _minMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private int? _moodLevelFilter;
    private string? _activityFilter;

    public EntriesView()
    {
        InitializeComponent();

        EntriesList.ItemsSource = _items;
        InitializeFilters();
        UpdateHeader();
        Render();
    }

    private void Render()
    {
        RenderCalendar();
        RenderEntries();
    }

    private void RenderEntries()
    {
        var entries = AppData.GetEntryPreviewsForMonth(_monthCursor);

        if (_moodLevelFilter.HasValue)
        {
            entries = entries.Where(e => e.MoodLevel == _moodLevelFilter.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(_activityFilter))
        {
            entries = entries.Where(e => e.Activities.Contains(_activityFilter)).ToList();
        }

        _items.Clear();
        foreach (var e in entries)
        {
            _items.Add(e);
        }

        if (EntriesEmptyStatePanel is not null)
        {
            EntriesEmptyStatePanel.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void InitializeFilters()
    {
        if (MoodFilterCombo is not null)
        {
            MoodFilterCombo.Items.Clear();
            MoodFilterCombo.Items.Add(new ComboBoxItem { Content = "Все настроения", Tag = null });
            MoodFilterCombo.Items.Add(new ComboBoxItem { Content = "😔 Плохо", Tag = 1 });
            MoodFilterCombo.Items.Add(new ComboBoxItem { Content = "😣 Ниже нормы", Tag = 2 });
            MoodFilterCombo.Items.Add(new ComboBoxItem { Content = "😐 Норм", Tag = 3 });
            MoodFilterCombo.Items.Add(new ComboBoxItem { Content = "🙂 Хорошо", Tag = 4 });
            MoodFilterCombo.Items.Add(new ComboBoxItem { Content = "😊 Отлично", Tag = 5 });
            MoodFilterCombo.SelectedIndex = 0;
        }

        RefreshActivityFilterItems(keepSelection: false);
    }

    private void RefreshActivityFilterItems(bool keepSelection)
    {
        if (ActivityFilterCombo is null)
        {
            return;
        }

        var prev = keepSelection ? _activityFilter : null;

        var activities = AppData.GetEntryPreviewsForMonth(_monthCursor)
            .SelectMany(e => e.Activities)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        ActivityFilterCombo.Items.Clear();
        ActivityFilterCombo.Items.Add(new ComboBoxItem { Content = "Все активности", Tag = null });
        foreach (var a in activities)
        {
            ActivityFilterCombo.Items.Add(new ComboBoxItem { Content = a, Tag = a });
        }

        if (!string.IsNullOrWhiteSpace(prev) && activities.Contains(prev))
        {
            foreach (var item in ActivityFilterCombo.Items)
            {
                if (item is ComboBoxItem { Tag: string t } && t == prev)
                {
                    ActivityFilterCombo.SelectedItem = item;
                    return;
                }
            }
        }

        ActivityFilterCombo.SelectedIndex = 0;
        _activityFilter = null;
    }

    private static Brush MoodBrush(int moodLevel)
    {
        var key = moodLevel switch
        {
            1 => "Brush.Mood.Bad",
            2 => "Brush.Mood.Low",
            3 => "Brush.Mood.Mid",
            4 => "Brush.Mood.Good",
            5 => "Brush.Mood.Great",
            _ => ""
        };

        if (key.Length > 0 && Application.Current.Resources[key] is SolidColorBrush scb)
        {
            return scb;
        }

        return (Brush)Application.Current.Resources["Brush.Surface2"];
    }

    private static Brush WithAlpha(Brush brush, byte alpha)
    {
        if (brush is SolidColorBrush scb)
        {
            return new SolidColorBrush(Color.FromArgb(alpha, scb.Color.R, scb.Color.G, scb.Color.B));
        }

        return brush;
    }

    private void RenderCalendar()
    {
        CalendarDaysGrid.Children.Clear();

        var firstOfMonth = new DateOnly(_monthCursor.Year, _monthCursor.Month, 1);
        var start = firstOfMonth;
        var offset = ((int)start.DayOfWeek + 6) % 7;
        var daysInMonth = DateTime.DaysInMonth(_monthCursor.Year, _monthCursor.Month);
        var requiredCells = offset + daysInMonth;
        var rows = requiredCells <= 35 ? 5 : 6;
        CalendarDaysGrid.Rows = rows;
        CalendarDaysGrid.Height = rows == 5 ? 300 : 360;

        start = start.AddDays(-offset);

        var today = DateOnly.FromDateTime(DateTime.Today);

        for (var i = 0; i < rows * 7; i++)
        {
            var date = start.AddDays(i);
            var isCurrentMonth = date.Month == firstOfMonth.Month;

            var moodLevel = AppData.GetMoodLevel(date);
            var moodBrush = moodLevel == 0 ? (Brush)Application.Current.Resources["Brush.Surface"] : MoodBrush(moodLevel);

            var cellBackground = moodLevel == 0
                ? (Brush)Application.Current.Resources["Brush.Surface"]
                : WithAlpha(moodBrush, 80);

            var cellBorder = moodLevel == 0
                ? (Brush)Application.Current.Resources["Brush.Border"]
                : WithAlpha(moodBrush, 140);

            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = isCurrentMonth ? cellBackground : (Brush)Application.Current.Resources["Brush.Surface2"],
                BorderBrush = cellBorder,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4)
            };

            border.Child = new TextBlock
            {
                Text = date.Day.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = isCurrentMonth
                    ? (Brush)Application.Current.Resources["Brush.Text"]
                    : (Brush)Application.Current.Resources["Brush.TextMuted"],
                FontWeight = isCurrentMonth ? FontWeights.SemiBold : FontWeights.Normal
            };

            border.Tag = date;

            if (isCurrentMonth && date <= today)
            {
                border.Cursor = Cursors.Hand;
                border.MouseLeftButtonUp += CalendarDayBorder_MouseLeftButtonUp;
            }

            CalendarDaysGrid.Children.Add(border);
        }
    }

    private void CalendarDayBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: DateOnly date })
        {
            return;
        }

        if (date > DateOnly.FromDateTime(DateTime.Today))
        {
            return;
        }

        var monthEntries = AppData.GetEntryPreviewsForMonth(_monthCursor);
        var first = monthEntries.FirstOrDefault(x => x.Date == date);
        if (first is not null)
        {
            UiServices.Navigation.Navigate(AppPage.EntryDetails, first);
            return;
        }

        UiServices.Navigation.Navigate(AppPage.AddEntry);
    }

    private void UpdateHeader()
    {
        MonthText.Text = _monthCursor.ToString("MMMM yyyy");
        var maxMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        NextMonthButton.IsEnabled = _monthCursor < maxMonth;
        PrevMonthButton.IsEnabled = _monthCursor > _minMonth;
    }

    private void PrevMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(-1);
        RefreshActivityFilterItems(keepSelection: true);
        UpdateHeader();
        Render();
    }

    private void NextMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(1);
        RefreshActivityFilterItems(keepSelection: true);
        UpdateHeader();
        Render();
    }

    private void MoodFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MoodFilterCombo?.SelectedItem is ComboBoxItem { Tag: int moodLevel })
        {
            _moodLevelFilter = moodLevel;
        }
        else
        {
            _moodLevelFilter = null;
        }

        RenderEntries();
    }

    private void ActivityFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActivityFilterCombo?.SelectedItem is ComboBoxItem { Tag: string activity } && !string.IsNullOrWhiteSpace(activity))
        {
            _activityFilter = activity;
        }
        else
        {
            _activityFilter = null;
        }

        RenderEntries();
    }

    private void CreateEntryButton_Click(object sender, RoutedEventArgs e)
    {
        UiServices.Navigation.Navigate(AppPage.AddEntry);
    }

    private void EntryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: EntryPreview entry })
        {
            UiServices.Navigation.Navigate(AppPage.EntryDetails, entry);
        }
    }
}
