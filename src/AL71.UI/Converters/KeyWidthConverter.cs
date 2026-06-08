using System.Globalization;
using System.Windows.Data;

namespace AL71.UI.Converters;

/// <summary>Converte la larghezza relativa di un tasto in pixel per la tastiera visuale.</summary>
public sealed class KeyWidthConverter : IValueConverter
{
    private const double Unit = 34.0;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double w ? w * Unit : Unit;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
