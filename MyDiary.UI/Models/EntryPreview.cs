namespace MyDiary.UI.Models;

public record EntryPreview(
    string Title,
    string Summary,
    string Mood,
    int MoodStatus,
    DateTime CreatedAt,
    string[] Activities
);
