using System;
using System.Collections.Generic;
using System.Linq;
using MyDiary.UI.Models;

namespace MyDiary.UI.Demo;

public record DemoDiaryEntry(
    DateOnly Date,
    DateTime CreatedAt,
    int MoodLevel,
    string MoodEmoji,
    string Title,
    string Summary,
    string[] Activities
);

public static class DemoData
{
    private static readonly IReadOnlyList<DemoDiaryEntry> _entries = Build();
    private static readonly IReadOnlyDictionary<DateOnly, int> _moodByDate = _entries
        .GroupBy(e => e.Date)
        .ToDictionary(g => g.Key, g => (int)Math.Round(g.Average(x => x.MoodLevel)));

    public static IReadOnlyList<DemoDiaryEntry> Entries => _entries;

    public static IReadOnlyList<DemoDiaryEntry> GetEntriesForMonth(DateTime month)
    {
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return _entries
            .Where(e => e.Date >= start && e.Date <= end)
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }

    public static IReadOnlyList<DemoDiaryEntry> GetEntriesForLastDays(int days)
    {
        var end = DateOnly.FromDateTime(DateTime.Today);
        var start = end.AddDays(-(days - 1));
        return _entries
            .Where(e => e.Date >= start && e.Date <= end)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.CreatedAt)
            .ToList();
    }

    public static IReadOnlyList<DemoDiaryEntry> GetEntriesForLastMonths(int months)
    {
        var end = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);
        var start = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-(months - 1));
        return _entries
            .Where(e => e.Date >= start && e.Date <= end)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.CreatedAt)
            .ToList();
    }

    public static int GetMoodLevel(DateOnly date)
    {
        return _moodByDate.TryGetValue(date, out var v) ? v : 0;
    }

    public static EntryPreview ToPreview(DemoDiaryEntry e)
    {
        return new EntryPreview(
            Title: e.Title,
            Summary: e.Summary,
            Mood: e.MoodEmoji,
            CreatedAt: e.CreatedAt,
            Activities: e.Activities
        );
    }

    public static (string Legend, int[] Series, string[] Labels) BuildSeriesForLastEntryDays(int maxDays)
    {
        var daysWithData = _entries
            .GroupBy(e => e.Date)
            .Select(g => new { Date = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.Date)
            .ToList();

        var take = Math.Min(maxDays, daysWithData.Count);
        var sliced = daysWithData.Skip(Math.Max(0, daysWithData.Count - take)).ToList();

        return (
            $"Серия: настроение по дням (последние {sliced.Count} дней с записями)",
            sliced.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            sliced.Select(x => $"{x.Date.Day:00}.{x.Date.Month:00}").ToArray()
        );
    }

    public static (string Legend, int[] Series, string[] Labels) BuildSeriesForLastWeeks(int maxWeeks)
    {
        var grouped = _entries
            .GroupBy(e => WeekStartMonday(e.Date))
            .Select(g => new { WeekStart = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.WeekStart)
            .ToList();

        var take = Math.Min(maxWeeks, grouped.Count);
        var sliced = grouped.Skip(Math.Max(0, grouped.Count - take)).ToList();

        return (
            $"Серия: среднее настроение по неделям (последние {sliced.Count})",
            sliced.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            sliced.Select(x => $"{x.WeekStart.Day:00}.{x.WeekStart.Month:00}").ToArray()
        );
    }

    public static (string Legend, int[] Series, string[] Labels) BuildSeriesForLastMonths(int months)
    {
        var startMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-(months - 1));

        var grouped = _entries
            .GroupBy(e => new DateOnly(e.Date.Year, e.Date.Month, 1))
            .Where(g => g.Key >= startMonth)
            .Select(g => new { Month = g.Key, Avg = (int)Math.Round(g.Average(x => x.MoodLevel)) })
            .OrderBy(x => x.Month)
            .ToList();

        return (
            "Серия: среднее настроение по месяцам",
            grouped.Select(x => Math.Clamp(x.Avg, 1, 5)).ToArray(),
            grouped.Select(x => x.Month.ToDateTime(TimeOnly.MinValue).ToString("MMM")).ToArray()
        );
    }

    public static Dictionary<int, int> CountMoodLevels(IEnumerable<DemoDiaryEntry> entries)
    {
        var dict = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var e in entries)
        {
            var k = Math.Clamp(e.MoodLevel, 1, 5);
            dict[k]++;
        }
        return dict;
    }

    private static DateOnly WeekStartMonday(DateOnly date)
    {
        var dow = (int)date.DayOfWeek;
        var offset = dow == 0 ? 6 : dow - 1;
        return date.AddDays(-offset);
    }

    private static IReadOnlyList<DemoDiaryEntry> Build()
    {
        var startMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-2);
        var end = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

        var titles = new[]
        {
            "Спокойный день",
            "Немного тревожно",
            "Хорошие новости",
            "Усталость",
            "Прогулка помогла",
            "Сфокусировался",
            "День без спешки",
            "Сложный день",
            "Нормальный ритм",
            "Отличное настроение"
        };

        var summaries = new[]
        {
            "День прошёл ровно. Сделал основные задачи, вечером удалось отдохнуть.",
            "Была небольшая тревога, но я заметил её и переключился на действия.",
            "Получил хорошие новости — настроение заметно улучшилось.",
            "Много дел подряд. Понял, что нужно восстановление и сон.",
            "После прогулки стало легче и спокойнее.",
            "Удалось держать фокус и закрыть то, что откладывал.",
            "Не торопился и делал всё по шагам — это помогло расслабиться.",
            "Стресс и дедлайны. Важно не забыть про отдых.",
            "Обычный день: без провалов и без пиков.",
            "Чувствую прилив сил и мотивации. Хочу закрепить этот настрой."
        };

        var activitiesPool = new[] { "спорт", "прогулка", "работа", "учёба", "дом", "друзья", "сон", "хобби", "музыка", "чтение" };
        var moodEmojiByLevel = new Dictionary<int, string>
        {
            { 1, "😔" },
            { 2, "😣" },
            { 3, "😐" },
            { 4, "🙂" },
            { 5, "😊" }
        };

        var list = new List<DemoDiaryEntry>();

        for (var d = startMonth; d <= end; d = d.AddDays(1))
        {
            var dayIndex = (d.DayNumber - startMonth.DayNumber);
            if (dayIndex % 4 == 0 || dayIndex % 11 == 0)
            {
                continue;
            }

            var moodLevel = 1 + Math.Abs(((d.DayNumber * 31) + (d.Month * 17) + d.Day) % 5);
            moodLevel = Math.Clamp(moodLevel, 1, 5);

            var title = titles[Math.Abs(d.DayNumber) % titles.Length];
            var summary = summaries[Math.Abs((d.DayNumber * 7) + d.Day) % summaries.Length];

            var a1 = activitiesPool[Math.Abs(d.DayNumber) % activitiesPool.Length];
            var a2 = activitiesPool[Math.Abs(d.DayNumber + 3) % activitiesPool.Length];
            var a3 = activitiesPool[Math.Abs(d.DayNumber + 6) % activitiesPool.Length];

            var createdAt = d.ToDateTime(new TimeOnly(9 + (dayIndex % 10), (dayIndex * 7) % 60));

            list.Add(new DemoDiaryEntry(
                Date: d,
                CreatedAt: createdAt,
                MoodLevel: moodLevel,
                MoodEmoji: moodEmojiByLevel[moodLevel],
                Title: title,
                Summary: summary,
                Activities: new[] { a1, a2, a3 }
            ));
        }

        return list;
    }
}
