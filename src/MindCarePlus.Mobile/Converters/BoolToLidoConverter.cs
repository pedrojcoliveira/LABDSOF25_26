using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class BoolToLidoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isLido)
            {
                return isLido ? "Lido" : "Novo";
            }
            return "Desconhecido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}