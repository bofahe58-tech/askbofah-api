using System.Globalization;

namespace AskBofah.Helpers
{
    public sealed class InverseBoolConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return value is bool boolValue &&
                   !boolValue;
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return value is bool boolValue &&
                   !boolValue;
        }
    }
}