using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyDiary.UI.Demo;
using MyDiary.UI.Models;
using MyDiary.UI.Navigation;

namespace MyDiary.UI.Views;

public partial class EntriesView : UserControl
{
    private DateTime _monthCursor = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private readonly ObservableCollection<EntryPreview> _items = new();
    private readonly DateTime _minMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public EntriesView()
    {
        InitializeComponent();

        _minMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2);

        EntriesList.ItemsSource = _items;
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
        var entries = DemoData.GetEntriesForMonth(_monthCursor);
        _items.Clear();
        foreach (var e in entries)
        {
            _items.Add(DemoData.ToPreview(e));
        }
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

        for (var i = 0; i < rows * 7; i++)
        {
            var date = start.AddDays(i);
            var isCurrentMonth = date.Month == firstOfMonth.Month;

            var moodLevel = DemoData.GetMoodLevel(date);
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

            CalendarDaysGrid.Children.Add(border);
        }
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
        UpdateHeader();
        Render();
    }

    private void NextMonthButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(1);
        UpdateHeader();
        Render();
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
