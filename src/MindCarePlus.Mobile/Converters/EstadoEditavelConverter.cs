using System.Globalization;

namespace MindCarePlus.Mobile.Converters;

public class EstadoEditavelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            // Só pode editar se não estiver realizada ou cancelada
            return estado != "Realizada" && estado != "Cancelada";
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EstadoCancelavelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string estado)
        {
            // Só pode cancelar se não estiver realizada ou já cancelada
            return estado != "Realizada" && estado != "Cancelada";
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}