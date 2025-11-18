// Em MindCarePlus.Mobile/Converters/PercentToProgressConverter.cs
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class PercentToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress / 100.0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}