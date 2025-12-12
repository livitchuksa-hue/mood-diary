using System.Windows.Controls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MyDiary.UI.Views;

public partial class CalendarView : UserControl
{
    private DateTime _monthCursor = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private int _weekCursor = 1;
    private bool _isInitialized;

    private static Brush MoodBrush(int moodLevel)
    {
        var key = moodLevel switch
        {
            1 => "Brush.Mood.Bad",
            2 => "Brush.Mood.Low",
            3 => "Brush.Mood.Mid",
            4 => "Brush.Mood.Good",
            _ => "Brush.Mood.Great"
        };

        if (Application.Current.Resources[key] is SolidColorBrush scb)
        {
            return scb;
        }

        return Brushes.LightGray;
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

        Loaded += CalendarView_Loaded;
    }

    private void CalendarView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        Render();
    }

    private void PrevButton_Click(object sender, RoutedEventArgs e)
    {
        if (DaysModeRadio.IsChecked == true)
        {
            _monthCursor = _monthCursor.AddMonths(-1);
        }
        else
        {
            _weekCursor = Math.Max(1, _weekCursor - 1);
        }

        Render();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (DaysModeRadio.IsChecked == true)
        {
            _monthCursor = _monthCursor.AddMonths(1);
        }
        else
        {
            _weekCursor = Math.Min(52, _weekCursor + 1);
        }

        Render();
    }

    private void ModeRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized)
        {
            return;
        }

        Render();
    }

    private void Render()
    {
        if (DaysModeRadio.IsChecked == true)
        {
            DaysGrid.Visibility = Visibility.Visible;
            WeeksGrid.Visibility = Visibility.Collapsed;
            PeriodText.Text = _monthCursor.ToString("MMMM yyyy");
            var maxMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            NextButton.IsEnabled = _monthCursor < maxMonth;
            RenderDays();
        }
        else
        {
            DaysGrid.Visibility = Visibility.Collapsed;
            WeeksGrid.Visibility = Visibility.Visible;
            PeriodText.Text = $"Неделя {_weekCursor}";
            var currentWeek = GetWeekNumber(DateTime.Today);
            NextButton.IsEnabled = _weekCursor < currentWeek;
            RenderWeeks();
        }

        PrevButton.IsEnabled = true;
    }

    private static int GetWeekNumber(DateTime date)
    {
        var cal = CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    private void RenderDays()
    {
        DaysGrid.Children.Clear();

        for (var i = 1; i <= 42; i++)
        {
            var dayNumber = i;

            var moodLevel = (i % 5) + 1;
            var mood = MoodBrush(moodLevel);

            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = WithAlpha(mood, 80),
                BorderBrush = WithAlpha(mood, 140),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4),
                Height = 58
            };

            border.Child = new TextBlock
            {
                Text = dayNumber.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["Brush.Text"]
            };

            DaysGrid.Children.Add(border);
        }
    }

    private void RenderWeeks()
    {
        WeeksGrid.Children.Clear();

        for (var i = 1; i <= 52; i++)
        {
            var moodLevel = (i % 5) + 1;
            var mood = MoodBrush(moodLevel);

            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Background = WithAlpha(mood, 70),
                BorderBrush = WithAlpha(mood, 130),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(4),
                Height = 34
            };

            border.Child = new TextBlock
            {
                Text = i.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (Brush)Application.Current.Resources["Brush.TextMuted"]
            };

            WeeksGrid.Children.Add(border);
        }
    }
}
