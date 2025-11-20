using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindCarePlus.Mobile.Converters;

public class InicialNomeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var nome = value as string;
        if (string.IsNullOrWhiteSpace(nome))
            return "?";

        // Pegar primeira letra de cada palavra
        var palavras = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (palavras.Length == 1)
            return palavras[0][0].ToString().ToUpper();

        return $"{palavras[0][0]}{palavras[palavras.Length - 1][0]}".ToUpper();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
