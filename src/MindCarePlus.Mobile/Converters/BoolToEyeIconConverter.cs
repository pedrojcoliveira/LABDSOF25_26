using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Converters;
public class BoolToEyeIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible && isVisible)
            return "eye_open.png"; // Ícone de olho aberto (password visível)
        return "eye_closed.png"; // Ícone de olho fechado (password escondida)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
