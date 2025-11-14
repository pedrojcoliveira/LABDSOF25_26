//Muda a cor da borda do desafio com base no estado.
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Se o estado for "Concluido", a borda fica Verde (podes mudar a cor)
            if (value as string == "Concluido")
            {
                return Colors.Green;
            }

            // Senão, fica cinzenta
            return Colors.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}