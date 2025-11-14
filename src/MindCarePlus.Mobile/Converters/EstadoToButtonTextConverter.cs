// Muda o texto do botão (ex: "Concluir" ou "Feito").
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string == "Concluido" ? "Feito ✓" : "Concluir";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}