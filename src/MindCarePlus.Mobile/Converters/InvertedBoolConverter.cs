using System.Globalization;

//"traduz" true para false e vice-versa. É usado para esconder a mensagem "Completa os 7 desafios" quando a semana já está completa.
namespace MindCarePlus.Mobile.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}