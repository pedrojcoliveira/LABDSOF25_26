// Em MindCarePlus.Mobile/Converters/EstadoToBorderColorConverter.cs
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Se "Concluido", borda verde
            if (value as string == "Concluido")
            {
                return Color.FromArgb("#10B981"); // Verde
            }

            // Senão ("Pendente"), borda cinzenta clara
            return Color.FromArgb("#E5E7EB");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}