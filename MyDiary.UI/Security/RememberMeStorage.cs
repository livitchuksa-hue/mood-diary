using System;
using MyDiary.UI.Properties;

namespace MyDiary.UI.Security;

public static class RememberMeStorage
{
    public static bool TryGetRememberedUserId(out Guid userId)
    {
        userId = default;

        try
        {
            var text = Settings.Default.RememberedUserId;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return Guid.TryParse(text, out userId);
        }
        catch
        {
            return false;
        }
    }

    public static void SaveUserId(Guid userId)
    {
        Settings.Default.RememberedUserId = userId.ToString();
        Settings.Default.Save();
    }

    public static void Clear()
    {
        try
        {
            Settings.Default.RememberedUserId = string.Empty;
            Settings.Default.Save();
        }
        catch
        {
        }
    }
}
