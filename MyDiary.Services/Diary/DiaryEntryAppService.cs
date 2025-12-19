using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Services.Diary;

public sealed record DiaryEntryPreviewDto(
    Guid Id,
    DateOnly Date,
    string Title,
    string Summary,
    string Content,
    int MoodStatus,
    DateTime CreatedAtUtc,
    string[] Activities);

public static class DiaryEntryAppService
{
    public static async Task<IReadOnlyList<DiaryEntryPreviewDto>> GetPreviewsForMonthAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        DateTime month,
        CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(month.Year, month.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var entries = await diaryEntryRepository.GetByUserAndPeriodAsync(userId, start, end, cancellationToken);
        return entries
            .OrderByDescending(x => x.Date)  // Сортировка по дате записи (новые сверху)
            .Select(ToPreview)
            .ToList();
    }

    public static async Task<IReadOnlyList<DiaryEntryPreviewDto>> GetPreviewsForDayAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        DateOnly day,
        CancellationToken cancellationToken = default)
    {
        var entries = await diaryEntryRepository.GetByUserAndPeriodAsync(userId, day, day, cancellationToken);
        return entries
            .OrderByDescending(x => x.Date)  // Сортировка по дате записи
            .Select(ToPreview)
            .ToList();
    }

    public static async Task<DiaryEntryPreviewDto?> GetPreviewByIdAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid entryId,
        CancellationToken cancellationToken = default)
    {
        var entry = await diaryEntryRepository.GetByIdAsync(entryId, cancellationToken);
        return entry is null ? null : ToPreview(entry);
    }

    public static async Task<IReadOnlyList<string>> GetAllActivityNamesAsync(
        IUserActivityRepository userActivityRepository,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var activities = await userActivityRepository.GetByUserIdAsync(userId, cancellationToken);
        return activities
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }

    public static async Task<Dictionary<DateOnly, int>> GetMoodLevelsByDateAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var entries = await diaryEntryRepository.GetByUserAndPeriodAsync(userId, start, end, cancellationToken);

        return entries
            .GroupBy(x => x.Date)
            .ToDictionary(
                g => g.Key,
                g => (int)Math.Round(g.Average(x => Math.Clamp(x.MoodStatus, 1, 5))));
    }

    public static async Task<IReadOnlyList<(DateOnly Date, int MoodLevel)>> GetMoodEntriesAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var entries = await diaryEntryRepository.GetByUserAndPeriodAsync(userId, start, end, cancellationToken);
        return entries
            .Select(x => (x.Date, Math.Clamp(x.MoodStatus, 1, 5)))
            .ToList();
    }

    public static async Task<Guid> CreateAsync(
        IDiaryEntryRepository diaryEntryRepository,
        IUserActivityRepository userActivityRepository,
        Guid userId,
        DateOnly date,
        string title,
        string content,
        int moodStatus,
        IReadOnlyList<string> activityNames,
        DateTime? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        var now = nowUtc ?? DateTime.UtcNow;
        
        // Отладка: выведем дату, которую используем для создания записи
        System.Diagnostics.Debug.WriteLine($"CreateAsync called with date: {date:dd.MM.yyyy}");
        
        var normalizedActivities = NormalizeActivityNames(activityNames);

        var activities = await EnsureActivitiesAsync(userActivityRepository, userId, normalizedActivities, cancellationToken);

        var entry = new DiaryEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = date,
            Title = (title ?? string.Empty).Trim(),
            Content = content ?? string.Empty,
            MoodStatus = Math.Clamp(moodStatus, 1, 5),
            CreatedAtUtc = now,
            UpdatedAtUtc = null,
            DiaryEntryActivities = activities
                .Select(a => new DiaryEntryActivity { DiaryEntryId = Guid.Empty, UserActivityId = a.Id })
                .ToList()
        };

        foreach (var link in entry.DiaryEntryActivities)
        {
            link.DiaryEntryId = entry.Id;
        }

        await diaryEntryRepository.AddAsync(entry, cancellationToken);
        
        // Отладка: выведем дату сохраненной записи
        System.Diagnostics.Debug.WriteLine($"Entry saved with ID={entry.Id}, Date={entry.Date:dd.MM.yyyy}");
        
        return entry.Id;
    }

    public static async Task<bool> UpdateAsync(
        IDiaryEntryRepository diaryEntryRepository,
        IUserActivityRepository userActivityRepository,
        Guid userId,
        Guid entryId,
        DateOnly date,
        string title,
        string content,
        int moodStatus,
        IReadOnlyList<string> activityNames,
        DateTime? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await diaryEntryRepository.GetByIdForUpdateAsync(entryId, cancellationToken);
        if (existing is null || existing.UserId != userId)
        {
            return false;
        }

        var now = nowUtc ?? DateTime.UtcNow;
        var normalizedActivities = NormalizeActivityNames(activityNames);
        var activities = await EnsureActivitiesAsync(userActivityRepository, userId, normalizedActivities, cancellationToken);

        existing.Date = date;
        existing.Title = (title ?? string.Empty).Trim();
        existing.Content = content ?? string.Empty;
        existing.MoodStatus = Math.Clamp(moodStatus, 1, 5);
        existing.UpdatedAtUtc = now;

        var targetIds = activities.Select(x => x.Id).ToHashSet();

        existing.DiaryEntryActivities.RemoveAll(x => !targetIds.Contains(x.UserActivityId));

        var existingIds = existing.DiaryEntryActivities.Select(x => x.UserActivityId).ToHashSet();
        foreach (var a in activities)
        {
            if (!existingIds.Contains(a.Id))
            {
                existing.DiaryEntryActivities.Add(new DiaryEntryActivity { DiaryEntryId = existing.Id, UserActivityId = a.Id });
            }
        }

        await diaryEntryRepository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    public static async Task DeleteAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        Guid entryId,
        CancellationToken cancellationToken = default)
    {
        var existing = await diaryEntryRepository.GetByIdAsync(entryId, cancellationToken);
        if (existing is null || existing.UserId != userId)
        {
            return;
        }

        await diaryEntryRepository.DeleteAsync(entryId, cancellationToken);
    }

    public static string MoodEmoji(int moodStatus)
    {
        return moodStatus switch
        {
            1 => "😣",
            2 => "😔",
            3 => "😐",
            4 => "🙂",
            _ => "😊"
        };
    }

    private static DiaryEntryPreviewDto ToPreview(DiaryEntry e)
    {
        var title = e.Title ?? string.Empty;
        var content = e.Content ?? string.Empty;
        var summary = content;
        if (summary.Length > 140)
        {
            summary = summary[..140] + "...";
        }

        var activities = e.DiaryEntryActivities
            .Select(x => x.UserActivity.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new DiaryEntryPreviewDto(
            e.Id,
            e.Date,
            title,
            summary,
            content,
            Math.Clamp(e.MoodStatus, 1, 5),
            e.CreatedAtUtc,
            activities);
    }

    private static List<string> NormalizeActivityNames(IReadOnlyList<string> activityNames)
    {
        return activityNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<List<UserActivity>> EnsureActivitiesAsync(
        IUserActivityRepository userActivityRepository,
        Guid userId,
        List<string> normalizedNames,
        CancellationToken cancellationToken)
    {
        if (normalizedNames.Count == 0)
        {
            return new List<UserActivity>();
        }

        var existing = await userActivityRepository.GetByUserIdAsync(userId, cancellationToken);

        var map = existing
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var result = new List<UserActivity>();
        foreach (var name in normalizedNames)
        {
            if (map.TryGetValue(name, out var a))
            {
                result.Add(a);
                continue;
            }

            var created = new UserActivity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = name,
                Description = string.Empty,
                DiaryEntryActivities = new List<DiaryEntryActivity>()
            };

            await userActivityRepository.AddAsync(created, cancellationToken);
            map[name] = created;
            result.Add(created);
        }

        return result;
    }
}
