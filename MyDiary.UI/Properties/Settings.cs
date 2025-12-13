using System.Configuration;

namespace MyDiary.UI.Properties;

internal sealed class Settings : ApplicationSettingsBase
{
    private static readonly Settings _default = (Settings)Synchronized(new Settings());

    public static Settings Default => _default;

    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string RememberedUserId
    {
        get => (string)this[nameof(RememberedUserId)];
        set => this[nameof(RememberedUserId)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("dark")]
    public string Theme
    {
        get => (string)this[nameof(Theme)];
        set => this[nameof(Theme)] = value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("ru")]
    public string Language
    {
        get => (string)this[nameof(Language)];
        set => this[nameof(Language)] = value;
    }
}
