using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MyDiary.UI.Themes;

public enum AppTheme
{
    Light = 0,
    Dark = 1
}

public static class ThemeManager
{
    private const string LightThemeSource = "Themes/LightTheme.xaml";
    private const string DarkThemeSource = "Themes/DarkTheme.xaml";

    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static AppTheme DetectCurrentTheme()
    {
        var app = Application.Current;
        if (app is null)
        {
            return CurrentTheme;
        }

        var themeDictionary = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source is not null && d.Source.OriginalString.Contains("Themes/", StringComparison.OrdinalIgnoreCase));

        var source = themeDictionary?.Source?.OriginalString ?? "";
        var theme = source.Contains("DarkTheme.xaml", StringComparison.OrdinalIgnoreCase)
            ? AppTheme.Dark
            : AppTheme.Light;

        CurrentTheme = theme;
        return theme;
    }

    public static void ApplyTheme(AppTheme theme)
    {
        var app = Application.Current;
        if (app is null)
        {
            return;
        }

        var targetSource = theme == AppTheme.Dark ? DarkThemeSource : LightThemeSource;
        var targetUri = new Uri(targetSource, UriKind.Relative);

        var dictionaries = app.Resources.MergedDictionaries;

        var existingThemeDictionaries = dictionaries
            .Where(d => d.Source is not null && d.Source.OriginalString.Contains("Themes/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var existing in existingThemeDictionaries)
        {
            dictionaries.Remove(existing);
        }

        dictionaries.Insert(0, new ResourceDictionary { Source = targetUri });
        SyncBrushColors(app.Resources);
        CurrentTheme = theme;
    }

    private static void SyncBrushColors(ResourceDictionary resources)
    {
        SyncBrushColor(resources, "Brush.Background", "Color.Background");
        SyncBrushColor(resources, "Brush.Surface", "Color.Surface");
        SyncBrushColor(resources, "Brush.Surface2", "Color.Surface2");
        SyncBrushColor(resources, "Brush.Border", "Color.Border");
        SyncBrushColor(resources, "Brush.Text", "Color.Text");
        SyncBrushColor(resources, "Brush.TextMuted", "Color.TextMuted");
        SyncBrushColor(resources, "Brush.Accent", "Color.Accent");
        SyncBrushColor(resources, "Brush.Hover", "Color.Hover");
        SyncBrushColor(resources, "Brush.AccentSoft", "Color.AccentSoft");

        SyncBrushColor(resources, "Brush.Mood.Bad", "Color.Mood.Bad");
        SyncBrushColor(resources, "Brush.Mood.Low", "Color.Mood.Low");
        SyncBrushColor(resources, "Brush.Mood.Mid", "Color.Mood.Mid");
        SyncBrushColor(resources, "Brush.Mood.Good", "Color.Mood.Good");
        SyncBrushColor(resources, "Brush.Mood.Great", "Color.Mood.Great");
    }

    private static void SyncBrushColor(ResourceDictionary resources, string brushKey, string colorKey)
    {
        if (resources[brushKey] is not SolidColorBrush brush)
        {
            return;
        }

        if (resources[colorKey] is not Color color)
        {
            return;
        }

        brush.Color = color;
    }
}
