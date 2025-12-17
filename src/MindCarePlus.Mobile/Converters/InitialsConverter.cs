using System.Globalization;

namespace MindCarePlus.Mobile.Converters;

public class InitialsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string name && !string.IsNullOrWhiteSpace(name))
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                // Primeira letra do primeiro nome + primeira letra do último nome
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
            else if (parts.Length == 1 && parts[0].Length >= 2)
            {
                // Primeiras duas letras do nome
                return parts[0].Substring(0, 2).ToUpper();
            }
            else if (parts.Length == 1 && parts[0].Length == 1)
            {
                // Apenas uma letra
                return parts[0][0].ToString().ToUpper();
            }
        }

        return "??";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
