using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Converters;

public class AlertaToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var alerta = value as string;
        return alerta?.ToLower() switch
        {
            "baixo" => Color.FromArgb("#10B981"),      // Verde
            "médio" or "medio" => Color.FromArgb("#F59E0B"),  // Amarelo
            "alto" => Color.FromArgb("#EF4444"),       // Vermelho
            _ => Color.FromArgb("#6B7280")             // Cinza default
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}