using System;

namespace MyDiary.UI.Models;

public record EntryPreview(
    Guid Id,
    DateOnly Date,
    string Title,
    string Summary,
    string Content,
    int MoodLevel,
    string Mood,
    DateTime CreatedAt,
    string[] Activities
)
{
    public int MoodStatus => MoodLevel;
}
