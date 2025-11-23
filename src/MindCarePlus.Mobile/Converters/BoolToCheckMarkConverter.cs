using System.Globalization;

namespace MindCarePlus.Mobile.Converters;

public class BoolToCheckMarkConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            return "✓✓"; // Duas marcas = lida
        }
        return "✓"; // Uma marca = enviada
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}