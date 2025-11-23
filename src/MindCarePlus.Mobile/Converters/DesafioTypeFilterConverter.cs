using System.Globalization;
using System.Collections;
using MindCarePlus.Shared.DTOs;

namespace MindCarePlus.Mobile.Converters
{
    public class DesafioTypeFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var todosDesafios = value as IEnumerable<DesafioAtribuidoDto>;
            var filtro = parameter as string; // "Terapeutico" ou "Diario"

            if (todosDesafios == null || filtro == null)
                return null;

            if (filtro == "Terapeutico")
            {
                // Retorna apenas as tarefas do psicólogo
                return todosDesafios.Where(d => d.Desafio.Tipo == "TarefaTerapeutica");
            }
            else
            {
                // Retorna tudo o resto (os desafios diários normais)
                return todosDesafios.Where(d => d.Desafio.Tipo != "TarefaTerapeutica");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}