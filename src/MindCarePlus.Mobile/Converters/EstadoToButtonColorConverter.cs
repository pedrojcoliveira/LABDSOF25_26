// Muda a cor de fundo do botão.
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoToButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Se "Concluido", fica Cinzento
            if (value as string == "Concluido")
            {
                return Colors.Gray;
            }

            // Senão ("Pendente"), fica Roxo (cor primária do teu Card)
            return Color.FromArgb("#7C3AED");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}