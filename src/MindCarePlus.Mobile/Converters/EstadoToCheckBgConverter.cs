// Em MindCarePlus.Mobile/Converters/EstadoToCheckBgConverter.cs
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoToCheckBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Se "Concluido", fundo verde
            if (value as string == "Concluido")
            {
                return Color.FromArgb("#10B981"); // Verde
            }

            // Senão ("Pendente"), fundo transparente
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}