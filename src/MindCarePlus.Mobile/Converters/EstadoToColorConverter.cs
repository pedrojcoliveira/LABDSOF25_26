using System.Globalization;

namespace MindCarePlus.Mobile.Converters;

public class EstadoToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            return estado switch
            {
                "Pendente" => Colors.Yellow,      // 🟨 Novo
                "Agendada" => Colors.Blue,
                "Cancelada" => Colors.Red,
                "Realizada" => Colors.Gray,
                "Faltou" => Colors.Orange,
                _ => Colors.LightGray
            };
        }
        return Colors.LightGray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}