namespace MyDiary.Domain.Entities;

public class DiaryEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int MoodStatus { get; set; }
    public List<DiaryEntryActivity> DiaryEntryActivities { get; set; } = new();
}
