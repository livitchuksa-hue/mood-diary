using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyDiary.Services.Diary;
using MyDiary.UI.Navigation;


namespace MyDiary.UI.Views;

public partial class CalendarView : UserControl
{
    private DateTime _monthCursor = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _isInitialized;
    private readonly DateTime _minMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    private Dictionary<DateOnly, int> _moodByDate = new();

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

    public CalendarView()
    {
        InitializeComponent();

        _minMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2);

        Loaded += CalendarView_Loaded;
    }

    private void CalendarView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _ = LoadAndRenderAsync();
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(-1);
        _ = LoadAndRenderAsync();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        _monthCursor = _monthCursor.AddMonths(1);
        _ = LoadAndRenderAsync();
    }

    private async System.Threading.Tasks.Task LoadAndRenderAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(Navigation.AppPage.Login);
            return;
        }

        PeriodText.Text = _monthCursor.ToString("MMMM yyyy");

        var maxMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        NextButton.IsEnabled = _monthCursor < maxMonth;
        PrevButton.IsEnabled = _monthCursor > _minMonth;

        await LoadMoodMapAsync();
        RenderDays();
    }

    private async System.Threading.Tasks.Task LoadMoodMapAsync()
    {
        var firstOfMonth = new DateOnly(_monthCursor.Year, _monthCursor.Month, 1);
        var offset = ((int)firstOfMonth.DayOfWeek + 6) % 7;
        var daysInMonth = DateTime.DaysInMonth(_monthCursor.Year, _monthCursor.Month);
        var requiredCells = offset + daysInMonth;
        var rows = requiredCells <= 35 ? 5 : 6;
        var gridStart = firstOfMonth.AddDays(-offset);
        var gridEnd = gridStart.AddDays(rows * 7 - 1);

        _moodByDate = await DiaryEntryAppService.GetMoodLevelsByDateAsync(
            UiServices.DiaryEntryRepository,
            UiServices.CurrentUser!.Id,
            gridStart,
            gridEnd);
    }

    private void RenderDays()
    {
        DaysGrid.Children.Clear();

        var firstOfMonth = new DateOnly(_monthCursor.Year, _monthCursor.Month, 1);
        var start = firstOfMonth;
        var offset = ((int)start.DayOfWeek + 6) % 7;
        var daysInMonth = DateTime.DaysInMonth(_monthCursor.Year, _monthCursor.Month);
        var requiredCells = offset + daysInMonth;
        var rows = requiredCells <= 35 ? 5 : 6;
        DaysGrid.Rows = rows;
        DaysGrid.Height = rows == 5 ? 300 : 360;

        start = start.AddDays(-offset);

        for (var i = 0; i < rows * 7; i++)
        {
            var date = start.AddDays(i);
            var isCurrentMonth = date.Month == firstOfMonth.Month;

            var moodLevel = _moodByDate.TryGetValue(date, out var ml) ? ml : 0;
            var mood = moodLevel == 0 ? (Brush)Application.Current.Resources["Brush.Surface"] : MoodBrush(moodLevel);

            var cellBackground = moodLevel == 0
                ? (Brush)Application.Current.Resources["Brush.Surface"]
                : WithAlpha(mood, 80);

            var cellBorder = moodLevel == 0
                ? (Brush)Application.Current.Resources["Brush.Border"]
                : WithAlpha(mood, 140);

            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = isCurrentMonth ? cellBackground : (Brush)Application.Current.Resources["Brush.Surface2"],
                BorderBrush = cellBorder,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4),
                Height = 58
            };

            border.Child = new TextBlock
            {
                Text = date.Day.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = isCurrentMonth
                    ? (Brush)Application.Current.Resources["Brush.Text"]
                    : (Brush)Application.Current.Resources["Brush.TextMuted"]
            };

            // Добавляем обработчик клика только для дней текущего месяца
            if (isCurrentMonth)
            {
                border.Cursor = System.Windows.Input.Cursors.Hand;
                var capturedDate = date; // Захватываем дату для замыкания
                border.MouseLeftButtonUp += (_, _) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Calendar clicked date: {capturedDate:dd.MM.yyyy}");
                    UiServices.Navigation.Navigate(Navigation.AppPage.AddEntry, capturedDate);
                };
            }

            DaysGrid.Children.Add(border);
        }
    }
}
