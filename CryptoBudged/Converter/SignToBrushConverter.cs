using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CryptoBudged.Converter
{
    [ValueConversion(typeof(int), typeof(Brush))]
    [ValueConversion(typeof(double), typeof(Brush))]
    [ValueConversion(typeof(byte), typeof(Brush))]
    [ValueConversion(typeof(long), typeof(Brush))]
    [ValueConversion(typeof(float), typeof(Brush))]
    [ValueConversion(typeof(uint), typeof(Brush))]
    [ValueConversion(typeof(short), typeof(Brush))]
    [ValueConversion(typeof(sbyte), typeof(Brush))]
    [ValueConversion(typeof(ushort), typeof(Brush))]
    [ValueConversion(typeof(ulong), typeof(Brush))]
    [ValueConversion(typeof(decimal), typeof(Brush))]
    public class SignToBrushConverter : IValueConverter
    {
        private static readonly Brush DefaultNegativeBrush = new SolidColorBrush(Colors.Red);
        private static readonly Brush DefaultPositiveBrush = new SolidColorBrush(Colors.Green);
        private static readonly Brush DefaultZeroBrush = new SolidColorBrush(Colors.Green);

        static SignToBrushConverter()
        {
            DefaultNegativeBrush.Freeze();
            DefaultPositiveBrush.Freeze();
            DefaultZeroBrush.Freeze();
        }

        public Brush NegativeBrush { get; set; }
        public Brush PositiveBrush { get; set; }
        public Brush ZeroBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!IsSupportedType(value)) return DependencyProperty.UnsetValue;

            double doubleValue = System.Convert.ToDouble(value);

            if (doubleValue < 0d) return NegativeBrush ?? DefaultNegativeBrush;
            if (doubleValue > 0d) return PositiveBrush ?? DefaultPositiveBrush;

            return ZeroBrush ?? DefaultZeroBrush;
        }

        private static bool IsSupportedType(object value)
        {
            return value is int || value is double || value is byte || value is long ||
                   value is float || value is uint || value is short || value is sbyte ||
                   value is ushort || value is ulong || value is decimal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
