// Em MindCarePlus.Mobile/Converters/EstadoIsConcluidoConverter.cs
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoIsConcluidoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Só mostra o "✓" se o estado for "Concluido"
            return value as string == "Concluido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}