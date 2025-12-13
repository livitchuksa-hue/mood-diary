using System;
using System.Collections.Generic;
using System.Linq;
using MyDiary.UI.Models;

namespace MyDiary.UI;

public static class AppData
{
    private sealed class AppDiaryEntry
    {
        public Guid Id { get; init; }
        public DateOnly Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public int MoodLevel { get; set; }
        public string[] Activities { get; set; } = Array.Empty<string>();
    }

    private static readonly List<AppDiaryEntry> _entries = new();

    private static readonly HashSet<string> _customActivities = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<string> GetAllActivities()
    {
        var fromEntries = _entries
            .SelectMany(e => e.Activities)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim());

        return fromEntries
            .Concat(_customActivities)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();
    }

    public static bool TryAddCustomActivity(string activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
        {
            return false;
        }

        var trimmed = activity.Trim();
        if (trimmed.Length == 0)
        {
            return false;
        }

        return _customActivities.Add(trimmed);
    }

    public static IReadOnlyList<EntryPreview> GetEntryPreviewsForMonth(DateTime month)
    {
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return _entries
            .Where(e => e.Date >= start && e.Date <= end)
            .OrderByDescending(e => e.CreatedAt)
            .Select(ToPreview)
            .ToList();
    }

    public static IReadOnlyList<EntryPreview> GetEntryPreviewsForDay(DateOnly day)
    {
        return _entries
            .Where(e => e.Date == day)
            .OrderByDescending(e => e.CreatedAt)
            .Select(ToPreview)
            .ToList();
    }

    public static IReadOnlyList<(DateOnly Date, int MoodLevel)> GetMoodEntries(DateOnly start, DateOnly end)
    {
        return _entries
            .Where(e => e.Date >= start && e.Date <= end)
            .Select(e => (e.Date, e.MoodLevel))
            .ToList();
    }

    public static int GetMoodLevel(DateOnly date)
    {
        var moods = _entries
            .Where(e => e.Date == date)
            .Select(e => e.MoodLevel)
            .ToList();

        if (moods.Count == 0)
        {
            return 0;
        }

        return (int)Math.Round(moods.Average());
    }

    public static EntryPreview? GetPreviewById(Guid id)
    {
        var e = _entries.FirstOrDefault(x => x.Id == id);
        return e is null ? null : ToPreview(e);
    }

    public static Guid AddEntry(DateOnly date, string title, int moodLevel, IEnumerable<string> activities, string content)
    {
        var id = Guid.NewGuid();
        _entries.Add(new AppDiaryEntry
        {
            Id = id,
            Date = date,
            CreatedAt = DateTime.Now,
            Title = title ?? "",
            Content = content ?? "",
            MoodLevel = Math.Clamp(moodLevel, 1, 5),
            Activities = activities?.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
                ?? Array.Empty<string>()
        });

        return id;
    }

    public static bool UpdateEntry(Guid id, DateOnly date, string title, int moodLevel, IEnumerable<string> activities, string content)
    {
        var e = _entries.FirstOrDefault(x => x.Id == id);
        if (e is null)
        {
            return false;
        }

        e.Date = date;
        e.Title = title ?? "";
        e.Content = content ?? "";
        e.MoodLevel = Math.Clamp(moodLevel, 1, 5);
        e.Activities = activities?.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            ?? Array.Empty<string>();

        return true;
    }

    public static bool DeleteEntry(Guid id)
    {
        var e = _entries.FirstOrDefault(x => x.Id == id);
        if (e is null)
        {
            return false;
        }

        _entries.Remove(e);
        return true;
    }

    public static Dictionary<int, int> CountMoodLevels(DateOnly start, DateOnly end)
    {
        var dict = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };

        foreach (var e in _entries.Where(x => x.Date >= start && x.Date <= end))
        {
            var k = Math.Clamp(e.MoodLevel, 1, 5);
            dict[k]++;
        }

        return dict;
    }

    public static string MoodEmoji(int moodLevel)
    {
        return moodLevel switch
        {
            1 => "😔",
            2 => "😣",
            3 => "😐",
            4 => "🙂",
            _ => "😊"
        };
    }

    private static EntryPreview ToPreview(AppDiaryEntry e)
    {
        var summary = e.Content ?? "";
        if (summary.Length > 140)
        {
            summary = summary[..140] + "...";
        }

        return new EntryPreview(
            Id: e.Id,
            Date: e.Date,
            Title: e.Title,
            Summary: summary,
            Content: e.Content,
            MoodLevel: e.MoodLevel,
            Mood: MoodEmoji(e.MoodLevel),
            CreatedAt: e.CreatedAt,
            Activities: e.Activities
        );
    }
}
