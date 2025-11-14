//Desativa o botão se o desafio já estiver concluído.
using System.Globalization;

namespace MindCarePlus.Mobile.Converters
{
    public class EstadoToButtonEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // O botão está ativo (Enabled=true) apenas se o estado NÃO for "Concluido"
            return value as string != "Concluido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}