using System;
using System.Linq;
using System.Windows;

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
        CurrentTheme = theme;
    }
}
