using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Converters
{
    public class IntToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedValue && parameter is string paramValue)
            {
                return selectedValue.ToString() == paramValue
                    ? Color.FromArgb("#7C3AED")  // Roxo quando selecionado
                    : Color.FromArgb("#E5E7EB"); // Cinza quando não selecionado
            }
            return Color.FromArgb("#E5E7EB");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
