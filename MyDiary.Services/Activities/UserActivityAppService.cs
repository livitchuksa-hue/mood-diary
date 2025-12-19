using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;

namespace MyDiary.Services.Activities;

public sealed record UserActivityDto(Guid Id, string Name, string Description);

public static class UserActivityAppService
{
    public static async Task<IReadOnlyList<UserActivityDto>> GetByUserIdAsync(
        IUserActivityRepository userActivityRepository,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var items = await userActivityRepository.GetByUserIdAsync(userId, cancellationToken);
        return items
            .OrderBy(x => x.Name)
            .Select(x => new UserActivityDto(x.Id, x.Name, x.Description))
            .ToList();
    }

    public static async Task<UserActivityDto?> GetByIdAsync(
        IUserActivityRepository userActivityRepository,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var a = await userActivityRepository.GetByIdAsync(id, cancellationToken);
        return a is null ? null : new UserActivityDto(a.Id, a.Name, a.Description);
    }

    public static async Task<UserActivityDto> CreateAsync(
        IUserActivityRepository userActivityRepository,
        Guid userId,
        string name,
        string description,
        CancellationToken cancellationToken = default)
    {
        name = (name ?? string.Empty).Trim();
        description ??= string.Empty;

        var activity = new UserActivity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = description,
            DiaryEntryActivities = new List<DiaryEntryActivity>()
        };

        await userActivityRepository.AddAsync(activity, cancellationToken);
        return new UserActivityDto(activity.Id, activity.Name, activity.Description);
    }

    public static async Task<bool> UpdateAsync(
        IUserActivityRepository userActivityRepository,
        Guid userId,
        Guid id,
        string name,
        string description,
        CancellationToken cancellationToken = default)
    {
        var existing = await userActivityRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null || existing.UserId != userId)
        {
            return false;
        }

        existing.Name = (name ?? string.Empty).Trim();
        existing.Description = description ?? string.Empty;

        await userActivityRepository.UpdateAsync(existing, cancellationToken);
        return true;
    }

    public static async Task<bool> DeleteAsync(
        IUserActivityRepository userActivityRepository,
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var existing = await userActivityRepository.GetByIdAsync(id, cancellationToken);
        if (existing is null || existing.UserId != userId)
        {
            return false;
        }

        await userActivityRepository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
