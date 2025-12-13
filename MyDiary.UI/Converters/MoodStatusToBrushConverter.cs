
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MyDiary.UI.Converters;

public sealed class MoodStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int moodLevel)
        {
            return (Brush)Application.Current.Resources["Brush.Surface2"];
        }

        var key = moodLevel switch
        {
            1 => "Brush.Mood.Bad",
            2 => "Brush.Mood.Low",
            3 => "Brush.Mood.Mid",
            4 => "Brush.Mood.Good",
            5 => "Brush.Mood.Great",
            _ => ""
        };

        if (key.Length == 0 || Application.Current.Resources[key] is not SolidColorBrush scb)
        {
            return (Brush)Application.Current.Resources["Brush.Surface2"];
        }

        return new SolidColorBrush(Color.FromArgb(60, scb.Color.R, scb.Color.G, scb.Color.B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
