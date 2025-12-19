using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using MyDiary.Services.Statistics;
using MyDiary.UI.Navigation;
using System.Reflection;

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

    private string _legend = string.Empty;
    private int[] _series = Array.Empty<int>();
    private string[] _labels = Array.Empty<string>();
    private string _desc = string.Empty;
    private Dictionary<int, int> _counts = new() { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };

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
        _appliedStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

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

        _ = LoadAndRenderAsync();
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

        _ = LoadAndRenderAsync();
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

        _ = LoadAndRenderAsync();
    }

    private async System.Threading.Tasks.Task LoadAndRenderAsync()
    {
        if (UiServices.CurrentUser is null)
        {
            UiServices.Navigation.Navigate(AppPage.Login);
            return;
        }

        var userId = UiServices.CurrentUser.Id;

        var mode = _mode switch
        {
            AggregationMode.Day => MoodAggregationMode.Day,
            AggregationMode.Week => MoodAggregationMode.Week,
            _ => MoodAggregationMode.Month
        };

        var series = await StatisticsAppService.GetMoodSeriesAsync(
            UiServices.DiaryEntryRepository,
            userId,
            _appliedStart,
            _appliedEnd,
            mode);

        _legend = series.Legend;
        _series = series.Series;
        _labels = series.Labels;

        _counts = await StatisticsAppService.CountMoodLevelsAsync(
            UiServices.DiaryEntryRepository,
            userId,
            _appliedStart,
            _appliedEnd);

        _desc = await StatisticsAppService.BuildActivityInsightAsync(
            UiServices.DiaryEntryRepository,
            userId,
            _appliedStart,
            _appliedEnd);
        DescText.Text = _desc;

        Render();
    }

    private void Render()
    {
        RenderLineChart();
        RenderPieChart();
    }

    private void RenderLineChart()
    {
        LineChartCanvas.Children.Clear();

        var legend = _legend;
        var series = _series;
        var labels = _labels;
        LineLegendText.Text = legend;

        if (LineEmptyStatePanel is not null)
        {
            LineEmptyStatePanel.Visibility = series.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        SyncKpis(series);

        if (series.Length == 0)
        {
            return;
        }

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
        var axisColor = axisBrush is SolidColorBrush ab ? ab.Color : Color.FromRgb(0, 0, 0);
        var gridBrush = new SolidColorBrush(Color.FromArgb(55, axisColor.R, axisColor.G, axisColor.B));
        var gridBrushMinor = new SolidColorBrush(Color.FromArgb(25, axisColor.R, axisColor.G, axisColor.B));
        var lineBrush = (Brush)Application.Current.Resources["Brush.Accent"];

        // Горизонтальная сетка: основные линии по уровням 1..5 + промежуточные линии (шаг 0.5)
        for (var k = 0; k <= 8; k++)
        {
            var y = paddingTop + plotH * (1.0 - (k / 8.0));
            var isMajor = k % 2 == 0;
            var grid = new Line
            {
                X1 = paddingLeft,
                X2 = paddingLeft + plotW,
                Y1 = y,
                Y2 = y,
                Stroke = isMajor ? gridBrush : gridBrushMinor,
                StrokeThickness = isMajor ? 1 : 0.75
            };
            LineChartCanvas.Children.Add(grid);

            if (isMajor)
            {
                var level = 1 + (k / 2);
                var yLabel = new TextBlock
                {
                    Text = level.ToString(),
                    Foreground = (Brush)Application.Current.Resources["Brush.TextMuted"],
                    FontSize = 12
                };
                Canvas.SetLeft(yLabel, 10);
                Canvas.SetTop(yLabel, y - 8);
                LineChartCanvas.Children.Add(yLabel);
            }
        }

        var n = Math.Max(2, series.Length);
        var maxXTicks = 7;
        var xStep = Math.Max(1, (int)Math.Ceiling(n / (double)maxXTicks));

        // Вертикальные тонкие линии для коротких серий, чтобы было легче видеть динамику
        if (n <= 20)
        {
            for (var i = 0; i < n; i++)
            {
                var x = paddingLeft + plotW * (i / (double)(n - 1));
                var vLine = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = paddingTop,
                    Y2 = paddingTop + plotH,
                    Stroke = gridBrushMinor,
                    StrokeThickness = 1
                };
                LineChartCanvas.Children.Add(vLine);
            }
        }

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

        // Сглаженная линия: Path с Bezier-сегментами (Catmull-Rom → Bezier)
        var points = new List<Point>(series.Length);
        for (var i = 0; i < series.Length; i++)
        {
            points.Add(Map(i, series[i]));
        }

        if (points.Count >= 2)
        {
            static Point C1(Point p0, Point p1, Point p2) => new(
                p1.X + (p2.X - p0.X) / 6.0,
                p1.Y + (p2.Y - p0.Y) / 6.0
            );

            static Point C2(Point p1, Point p2, Point p3) => new(
                p2.X - (p3.X - p1.X) / 6.0,
                p2.Y - (p3.Y - p1.Y) / 6.0
            );

            var fig = new PathFigure { StartPoint = points[0], IsClosed = false, IsFilled = false };
            for (var i = 0; i < points.Count - 1; i++)
            {
                var p0 = i == 0 ? points[0] : points[i - 1];
                var p1 = points[i];
                var p2 = points[i + 1];
                var p3 = i + 2 < points.Count ? points[i + 2] : points[^1];

                fig.Segments.Add(new BezierSegment(C1(p0, p1, p2), C2(p1, p2, p3), p2, true));
            }

            var path = new Path
            {
                Data = new PathGeometry(new[] { fig }),
                Stroke = lineBrush,
                StrokeThickness = 2.5,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            LineChartCanvas.Children.Add(path);
        }

        for (var i = 0; i < series.Length; i++)
        {
            var p = Map(i, series[i]);
            var dot = new Ellipse
            {
                Width = 7,
                Height = 7,
                Fill = (Brush)Application.Current.Resources["Brush.Surface"],
                Stroke = lineBrush,
                StrokeThickness = 2
            };

            var label = i < labels.Length ? labels[i] : (i + 1).ToString();
            dot.ToolTip = $"{label}\n{MoodStatisticsService.MoodLabel(series[i])} ({series[i]}/5)";

            Canvas.SetLeft(dot, p.X - dot.Width / 2);
            Canvas.SetTop(dot, p.Y - dot.Height / 2);
            LineChartCanvas.Children.Add(dot);
        }
    }

    private void SyncKpis(int[] series)
    {
        if (KpiAvgMoodText is null || KpiTrendText is null || KpiMinMaxText is null || KpiPointsText is null)
        {
            return;
        }

        var kpis = MoodStatisticsService.BuildKpis(series);
        KpiAvgMoodText.Text = kpis.AvgText;
        KpiTrendText.Text = kpis.TrendText;
        KpiMinMaxText.Text = kpis.MinMaxText;
        KpiPointsText.Text = kpis.PointsText;
    }


    private void RenderPieChart()
    {
        PieChartCanvas.Children.Clear();

        var counts = _counts;
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
            if (PieEmptyStatePanel is not null)
            {
                PieEmptyStatePanel.Visibility = Visibility.Visible;
            }
            PieCenterText.Text = "0";
            return;
        }

        if (PieEmptyStatePanel is not null)
        {
            PieEmptyStatePanel.Visibility = Visibility.Collapsed;
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
    
