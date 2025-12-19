using System.Globalization;

namespace MyDiary.Services.Statistics;

public enum MoodAggregationMode
{
    Day,
    Week,
    Month
}

public sealed record MoodSeriesResult(
    string Legend,
    int[] Series,
    string[] Labels);

public sealed record MoodKpiResult(
    string AvgText,
    string TrendText,
    string MinMaxText,
    string PointsText);

public static class MoodStatisticsService
{
    public static MoodSeriesResult BuildSeries(IReadOnlyList<(DateOnly Date, int MoodLevel)> entries, MoodAggregationMode mode)
    {
        if (entries.Count == 0)
        {
            return new MoodSeriesResult("Нет данных за выбранный период", Array.Empty<int>(), Array.Empty<string>());
        }

        return mode switch
        {
            MoodAggregationMode.Day => BuildSeriesByDay(entries),
            MoodAggregationMode.Week => BuildSeriesByWeek(entries),
            _ => BuildSeriesByMonth(entries)
        };
    }

    public static MoodKpiResult BuildKpis(int[] series)
    {
        if (series.Length == 0)
        {
            return new MoodKpiResult("—", "—", "—", "0");
        }

        var avg = series.Average();
        var min = series.Min();
        var max = series.Max();

        var first = series.First();
        var last = series.Last();
        var delta = last - first;

        var trend = delta switch
        {
            > 0 => $"↑ +{delta}",
            < 0 => $"↓ {delta}",
            _ => "→ 0"
        };

        return new MoodKpiResult(
            AvgText: $"{avg:0.0}/5",
            TrendText: trend,
            MinMaxText: $"{min}/{max}",
            PointsText: series.Length.ToString(CultureInfo.InvariantCulture)
        );
    }

    public static string MoodLabel(int mood)
    {
        return mood switch
        {
            1 => "Плохо",
            2 => "Ниже нормы",
            3 => "Среднее",
            4 => "Хорошо",
            5 => "Отлично",
            _ => string.Empty
        };
    }

    private static MoodSeriesResult BuildSeriesByDay(IReadOnlyList<(DateOnly Date, int MoodLevel)> entries)
    {
        var grouped = entries
            .GroupBy(e => e.Date)
            .Select(g => new { Date = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.Date)
            .ToList();

        return new MoodSeriesResult(
            Legend: "Серия: настроение по дням",
            Series: grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            Labels: grouped.Select(x => x.Date.ToDateTime(TimeOnly.MinValue).ToString("dd.MM"))
                .ToArray()
        );
    }

    private static MoodSeriesResult BuildSeriesByWeek(IReadOnlyList<(DateOnly Date, int MoodLevel)> entries)
    {
        var grouped = entries
            .GroupBy(e => WeekStartMonday(e.Date))
            .Select(g => new { WeekStart = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.WeekStart)
            .ToList();

        return new MoodSeriesResult(
            Legend: "Серия: среднее настроение по неделям",
            Series: grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            Labels: grouped.Select(x => x.WeekStart.ToDateTime(TimeOnly.MinValue).ToString("dd.MM"))
                .ToArray()
        );
    }

    private static MoodSeriesResult BuildSeriesByMonth(IReadOnlyList<(DateOnly Date, int MoodLevel)> entries)
    {
        var grouped = entries
            .GroupBy(e => new DateOnly(e.Date.Year, e.Date.Month, 1))
            .Select(g => new { Month = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.Month)
            .ToList();

        return new MoodSeriesResult(
            Legend: "Серия: среднее настроение по месяцам",
            Series: grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            Labels: grouped.Select(x => x.Month.ToDateTime(TimeOnly.MinValue).ToString("MMM"))
                .ToArray()
        );
    }

    private static DateOnly WeekStartMonday(DateOnly date)
    {
        var dow = (int)date.DayOfWeek;
        var offset = dow == 0 ? 6 : dow - 1;
        return date.AddDays(-offset);
    }
}
