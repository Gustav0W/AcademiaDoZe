using System.Globalization;

namespace AcademiaDoZe.Presentation.AppMaui.Converters;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(value as bool? ?? false);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(value as bool? ?? false);
    }
}