namespace MyDiary.UI.Models;

public record EntryPreview(
    string Title,
    string Summary,
    string Mood,
    DateTime CreatedAt,
    string[] Activities
);
