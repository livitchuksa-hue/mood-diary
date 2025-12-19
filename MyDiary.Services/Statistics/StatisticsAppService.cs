using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Services.Diary;

namespace MyDiary.Services.Statistics;

public static class StatisticsAppService
{
    public static async Task<(string Legend, int[] Series, string[] Labels)> GetMoodSeriesAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        DateOnly start,
        DateOnly end,
        MoodAggregationMode mode,
        CancellationToken cancellationToken = default)
    {
        var filtered = await DiaryEntryAppService.GetMoodEntriesAsync(diaryEntryRepository, userId, start, end, cancellationToken);
        var result = MoodStatisticsService.BuildSeries(filtered, mode);
        return (result.Legend, result.Series, result.Labels);
    }

    public static async Task<Dictionary<int, int>> CountMoodLevelsAsync(
        IDiaryEntryRepository diaryEntryRepository,
        Guid userId,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
    {
        var filtered = await DiaryEntryAppService.GetMoodEntriesAsync(diaryEntryRepository, userId, start, end, cancellationToken);

        var dict = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
        foreach (var (_, mood) in filtered)
        {
            dict[Math.Clamp(mood, 1, 5)]++;
        }

        return dict;
    }

    ////////////////////////////////////////////////////////////
    /// 
    public static async Task<string> BuildActivityInsightAsync(
    IDiaryEntryRepository diaryEntryRepository,
    Guid userId,
    DateOnly start,
    DateOnly end,
    CancellationToken ct = default)
{
    var entries = await diaryEntryRepository.GetByUserAndPeriodAsync(userId, start, end, ct);
    if (entries.Count < 5)
        return "Недостаточно данных за выбранный период, чтобы найти связь настроения и активностей.";

    // Среднее настроение по всем записям
    double overallAvg = entries.Average(e => Math.Clamp(e.MoodStatus, 1, 5));

    // Собираем множество активностей
    var allActivities = entries
        .SelectMany(e => e.DiaryEntryActivities.Select(x => x.UserActivity.Name))
        .Where(n => !string.IsNullOrWhiteSpace(n))
        .Select(n => n.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();

    if (allActivities.Count == 0)
        return "За выбранный период у записей нет активностей — добавляй активности, чтобы видеть влияние на настроение.";

    // Для каждой активности считаем delta = avgWith - avgWithout
    var scored = new List<(string Name, double AvgWith, double AvgWithout, double Delta, int WithCount, int WithoutCount)>();

    foreach (var a in allActivities)
    {
        var with = entries.Where(e =>
            e.DiaryEntryActivities.Any(x => string.Equals(x.UserActivity.Name, a, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var without = entries.Where(e =>
            !e.DiaryEntryActivities.Any(x => string.Equals(x.UserActivity.Name, a, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // чтобы не ловить “ложные” выводы на 1-2 записях
        if (with.Count < 2 || without.Count < 2)
            continue;

        var avgWith = with.Average(e => Math.Clamp(e.MoodStatus, 1, 5));
        var avgWithout = without.Average(e => Math.Clamp(e.MoodStatus, 1, 5));
        var delta = avgWith - avgWithout;

        scored.Add((a, avgWith, avgWithout, delta, with.Count, without.Count));
    }

    if (scored.Count == 0)
        return "Недостаточно данных по активностям (слишком мало записей с/без каждой активности), чтобы сделать вывод.";

    var best = scored.OrderByDescending(x => x.Delta).First();
    var worst = scored.OrderBy(x => x.Delta).First();

    // Порог, чтобы не говорить “сильно влияет”, когда разница почти нулевая
    const double minEffect = 0.25;

    string bestPhrase = best.Delta >= minEffect
        ? $"Занимаясь *{best.Name}* чаще, у вас в среднем выше настроение (≈ {best.AvgWith:0.0} против {best.AvgWithout:0.0})."
        : $"Активность *{best.Name}* выглядит немного полезнее для настроения (разница ≈ {best.Delta:+0.0;-0.0;0.0}).";

    string worstPhrase = worst.Delta <= -minEffect
        ? $"Попробуйте уменьшить количество *{worst.Name}* — с ней у вас настроение ниже (≈ {worst.AvgWith:0.0} против {worst.AvgWithout:0.0})."
        : $"Активность *{worst.Name}* выглядит немного хуже по настроению (разница ≈ {worst.Delta:+0.0;-0.0;0.0}).";

    return $"{bestPhrase} {worstPhrase}";
}
}
