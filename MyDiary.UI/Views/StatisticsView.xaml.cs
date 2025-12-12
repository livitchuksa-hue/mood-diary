using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using MyDiary.UI.Demo;

namespace MyDiary.UI.Views;

public partial class StatisticsView : UserControl
{
    private enum AggregationMode
    {
        Day,
        Week,
        Month
    }

    private bool _isInitialized;
    private AggregationMode _mode = AggregationMode.Day;

    private DateOnly _appliedStart;
    private DateOnly _appliedEnd;

    public StatisticsView()
    {
        InitializeComponent();

        Loaded += StatisticsView_Loaded;
    }

    private void StatisticsView_Loaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        var today = DateOnly.FromDateTime(DateTime.Today);
        _appliedEnd = today;
        _appliedStart = today.AddMonths(-2);

        if (StartDatePicker is not null)
        {
            StartDatePicker.SelectedDate = _appliedStart.ToDateTime(TimeOnly.MinValue);
        }

        if (EndDatePicker is not null)
        {
            EndDatePicker.SelectedDate = _appliedEnd.ToDateTime(TimeOnly.MinValue);
        }

        LineChartCanvas.SizeChanged += (_, _) => Render();
        PieChartCanvas.SizeChanged += (_, _) => Render();

        Render();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized)
        {
            return;
        }

        var startDt = StartDatePicker?.SelectedDate;
        var endDt = EndDatePicker?.SelectedDate;

        if (startDt is null || endDt is null)
        {
            return;
        }

        var start = DateOnly.FromDateTime(startDt.Value);
        var end = DateOnly.FromDateTime(endDt.Value);

        if (end < start)
        {
            (start, end) = (end, start);
        }

        _appliedStart = start;
        _appliedEnd = end;

        Render();
    }

    private void ModeRadio_Checked(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized)
        {
            return;
        }

        if (DayModeRadio.IsChecked == true)
        {
            _mode = AggregationMode.Day;
        }
        else if (WeekModeRadio.IsChecked == true)
        {
            _mode = AggregationMode.Week;
        }
        else
        {
            _mode = AggregationMode.Month;
        }

        Render();
    }

    private void Render()
    {
        RenderLineChart();
        RenderPieChart();
    }

    private (string Legend, int[] Series, string[] Labels) GetSeries()
    {
        var filtered = DemoData.Entries
            .Where(e => e.Date >= _appliedStart && e.Date <= _appliedEnd)
            .ToList();

        if (filtered.Count == 0)
        {
            return ("Нет данных за выбранный период", Array.Empty<int>(), Array.Empty<string>());
        }

        return _mode switch
        {
            AggregationMode.Day => BuildSeriesByDay(filtered),
            AggregationMode.Week => BuildSeriesByWeek(filtered),
            _ => BuildSeriesByMonth(filtered)
        };
    }

    private static (string Legend, int[] Series, string[] Labels) BuildSeriesByDay(IReadOnlyList<DemoDiaryEntry> entries)
    {
        var grouped = entries
            .GroupBy(e => e.Date)
            .Select(g => new { Date = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.Date)
            .ToList();

        return (
            "Серия: настроение по дням",
            grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            grouped.Select(x => x.Date.ToDateTime(TimeOnly.MinValue).ToString("dd.MM"))
                .ToArray()
        );
    }

    private static (string Legend, int[] Series, string[] Labels) BuildSeriesByWeek(IReadOnlyList<DemoDiaryEntry> entries)
    {
        var grouped = entries
            .GroupBy(e => WeekStartMonday(e.Date))
            .Select(g => new { WeekStart = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.WeekStart)
            .ToList();

        return (
            "Серия: среднее настроение по неделям",
            grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            grouped.Select(x => x.WeekStart.ToDateTime(TimeOnly.MinValue).ToString("dd.MM"))
                .ToArray()
        );
    }

    private static (string Legend, int[] Series, string[] Labels) BuildSeriesByMonth(IReadOnlyList<DemoDiaryEntry> entries)
    {
        var grouped = entries
            .GroupBy(e => new DateOnly(e.Date.Year, e.Date.Month, 1))
            .Select(g => new { Month = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.Month)
            .ToList();

        return (
            "Серия: среднее настроение по месяцам",
            grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            grouped.Select(x => x.Month.ToDateTime(TimeOnly.MinValue).ToString("MMM"))
                .ToArray()
        );
    }

    private static DateOnly WeekStartMonday(DateOnly date)
    {
        var dow = (int)date.DayOfWeek;
        var offset = dow == 0 ? 6 : dow - 1;
        return date.AddDays(-offset);
    }

    private static string MoodLabel(int mood)
    {
        return mood switch
        {
            1 => "Плохо",
            2 => "Ниже нормы",
            3 => "Норм",
            4 => "Хорошо",
            5 => "Отлично",
            _ => ""
        };
    }

    private static Dictionary<int, int> CountMoods(IEnumerable<int> series)
    {
        var dict = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var v in series)
        {
            var k = Math.Clamp(v, 1, 5);
            dict[k]++;
        }
        return dict;
    }

    private void RenderLineChart()
    {
        LineChartCanvas.Children.Clear();

        var (legend, series, labels) = GetSeries();
        LineLegendText.Text = legend;

        var w = Math.Max(1, LineChartCanvas.ActualWidth);
        var h = Math.Max(1, LineChartCanvas.ActualHeight);
        if (w <= 2 || h <= 2)
        {
            return;
        }

        var paddingLeft = 42.0;
        var paddingRight = 18.0;
        var paddingTop = 18.0;
        var paddingBottom = 34.0;
        var padding = 18.0;
        var plotW = Math.Max(1, w - paddingLeft - paddingRight);
        var plotH = Math.Max(1, h - paddingTop - paddingBottom);

        var axisBrush = (Brush)Application.Current.Resources["Brush.Border"];
        var gridBrush = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
        var lineBrush = (Brush)Application.Current.Resources["Brush.Accent"];

        for (var i = 0; i <= 4; i++)
        {
            var y = paddingTop + plotH * (i / 4.0);
            var grid = new Line
            {
                X1 = paddingLeft,
                X2 = paddingLeft + plotW,
                Y1 = y,
                Y2 = y,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            LineChartCanvas.Children.Add(grid);

            var yLabel = new TextBlock
            {
                Text = (5 - i).ToString(),
                Foreground = (Brush)Application.Current.Resources["Brush.TextMuted"],
                FontSize = 12
            };
            Canvas.SetLeft(yLabel, 10);
            Canvas.SetTop(yLabel, y - 8);
            LineChartCanvas.Children.Add(yLabel);
        }

        var n = Math.Max(2, series.Length);
        var maxXTicks = 7;
        var xStep = Math.Max(1, (int)Math.Ceiling(n / (double)maxXTicks));
        for (var i = 0; i < n; i += xStep)
        {
            var x = paddingLeft + plotW * (i / (double)(n - 1));
            var vLine = new Line
            {
                X1 = x,
                X2 = x,
                Y1 = paddingTop,
                Y2 = paddingTop + plotH,
                Stroke = gridBrush,
                StrokeThickness = 1
            };
            LineChartCanvas.Children.Add(vLine);

            if (i < labels.Length)
            {
                var xLabel = new TextBlock
                {
                    Text = labels[i],
                    Foreground = (Brush)Application.Current.Resources["Brush.TextMuted"],
                    FontSize = 12
                };
                Canvas.SetLeft(xLabel, x - 10);
                Canvas.SetTop(xLabel, paddingTop + plotH + 8);
                LineChartCanvas.Children.Add(xLabel);
            }
        }

        LineChartCanvas.Children.Add(new Rectangle
        {
            Width = plotW,
            Height = plotH,
            Stroke = axisBrush,
            StrokeThickness = 1,
            RadiusX = 10,
            RadiusY = 10
        });
        Canvas.SetLeft(LineChartCanvas.Children[^1], paddingLeft);
        Canvas.SetTop(LineChartCanvas.Children[^1], paddingTop);

        if (series.Length < 2)
        {
            return;
        }

        Point Map(int idx, int value)
        {
            var x = paddingLeft + plotW * (idx / (double)(series.Length - 1));
            var t = (value - 1) / 4.0;
            var y = paddingTop + plotH * (1.0 - t);
            return new Point(x, y);
        }

        var polyline = new Polyline
        {
            Stroke = lineBrush,
            StrokeThickness = 3,
            StrokeLineJoin = PenLineJoin.Round
        };

        for (var i = 0; i < series.Length; i++)
        {
            polyline.Points.Add(Map(i, series[i]));
        }
        LineChartCanvas.Children.Add(polyline);

        for (var i = 0; i < series.Length; i++)
        {
            var p = Map(i, series[i]);
            var dot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = (Brush)Application.Current.Resources["Brush.Surface"],
                Stroke = lineBrush,
                StrokeThickness = 2
            };

            var label = i < labels.Length ? labels[i] : (i + 1).ToString();
            dot.ToolTip = $"{label}\n{MoodLabel(series[i])} ({series[i]}/5)";

            Canvas.SetLeft(dot, p.X - dot.Width / 2);
            Canvas.SetTop(dot, p.Y - dot.Height / 2);
            LineChartCanvas.Children.Add(dot);
        }
    }

    private void RenderPieChart()
    {
        PieChartCanvas.Children.Clear();

        var entries = DemoData.Entries
            .Where(e => e.Date >= _appliedStart && e.Date <= _appliedEnd)
            .ToList();

        var counts = DemoData.CountMoodLevels(entries);
        PieLegendPanel.Children.Clear();

        var w = Math.Max(1, PieChartCanvas.ActualWidth);
        var h = Math.Max(1, PieChartCanvas.ActualHeight);
        if (w <= 2 || h <= 2)
        {
            return;
        }

        var cx = w / 2.0;
        var cy = h / 2.0;
        var rOuter = Math.Min(w, h) / 2.0;
        var rInner = rOuter * 0.62;

        var data = new List<(string Label, double Value, Brush Brush)>
        {
            ("Плохо", counts[1], (Brush)Application.Current.Resources["Brush.Mood.Bad"]),
            ("Ниже нормы", counts[2], (Brush)Application.Current.Resources["Brush.Mood.Low"]),
            ("Норм", counts[3], (Brush)Application.Current.Resources["Brush.Mood.Mid"]),
            ("Хорошо", counts[4], (Brush)Application.Current.Resources["Brush.Mood.Good"]),
            ("Отлично", counts[5], (Brush)Application.Current.Resources["Brush.Mood.Great"])
        };

        var total = data.Sum(x => x.Value);
        if (total <= 0)
        {
            PieCenterText.Text = "0";
            return;
        }

        var startAngle = -90.0;
        foreach (var part in data.Where(d => d.Value > 0))
        {
            var sweep = part.Value / total * 360.0;
            var path = CreateDonutSlice(cx, cy, rOuter, rInner, startAngle, startAngle + sweep);
            path.Fill = part.Brush;
            path.Stroke = (Brush)Application.Current.Resources["Brush.Surface2"];
            path.StrokeThickness = 1;
            PieChartCanvas.Children.Add(path);
            startAngle += sweep;
        }

        var top = data.OrderByDescending(x => x.Value).First();
        PieCenterText.Text = $"{(int)Math.Round(top.Value)}\n{top.Label}";

        foreach (var part in data)
        {
            var pct = total > 0 ? (part.Value / total) * 100.0 : 0.0;
            var row = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var swatch = new Border
            {
                Width = 12,
                Height = 12,
                CornerRadius = new CornerRadius(3),
                Background = part.Brush,
                BorderBrush = (Brush)Application.Current.Resources["Brush.Border"],
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(swatch, 0);
            row.Children.Add(swatch);

            var label = new TextBlock
            {
                Text = part.Label,
                Foreground = (Brush)Application.Current.Resources["Brush.Text"],
                FontSize = 12
            };
            Grid.SetColumn(label, 2);
            row.Children.Add(label);

            var value = new TextBlock
            {
                Text = $"{part.Value:0} ({pct:0}%)",
                Foreground = (Brush)Application.Current.Resources["Brush.TextMuted"],
                FontSize = 12
            };
            Grid.SetColumn(value, 3);
            row.Children.Add(value);

            PieLegendPanel.Children.Add(row);
        }
    }

    private static Path CreateDonutSlice(double cx, double cy, double rOuter, double rInner, double startAngleDeg, double endAngleDeg)
    {
        var startRad = startAngleDeg * Math.PI / 180.0;
        var endRad = endAngleDeg * Math.PI / 180.0;
        var largeArc = (endAngleDeg - startAngleDeg) > 180.0;

        Point Pt(double r, double a) => new(cx + r * Math.Cos(a), cy + r * Math.Sin(a));

        var p1 = Pt(rOuter, startRad);
        var p2 = Pt(rOuter, endRad);
        var p3 = Pt(rInner, endRad);
        var p4 = Pt(rInner, startRad);

        var fig = new PathFigure { StartPoint = p1, IsClosed = true, IsFilled = true };
        fig.Segments.Add(new ArcSegment(p2, new Size(rOuter, rOuter), 0, largeArc, SweepDirection.Clockwise, true));
        fig.Segments.Add(new LineSegment(p3, true));
        fig.Segments.Add(new ArcSegment(p4, new Size(rInner, rInner), 0, largeArc, SweepDirection.Counterclockwise, true));

        var geom = new PathGeometry();
        geom.Figures.Add(fig);

        return new Path { Data = geom };
    }
}
