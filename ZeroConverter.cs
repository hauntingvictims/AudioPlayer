using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AudioPlayer
{
    public class ZeroConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                bool isZero = intValue == 0;
                return Invert ? !isZero : isZero;
            }
            return false; // Повертаємо false за замовчуванням для нечислових значень
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}