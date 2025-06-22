using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Unibot.Converters;

public static class ValueConverters
{
    public static readonly SliderWidthConverter SliderWidthConverter = new();
    public static readonly BooleanToVisibilityConverter BooleanToVisibilityConverter = new();
    public static readonly InvertBooleanConverter InvertBooleanConverter = new();
    public static readonly EnumToStringConverter EnumToStringConverter = new();
    public static readonly DoubleToStringConverter DoubleToStringConverter = new();
    public static readonly EnumToVisibilityConverter EnumToVisibilityConverter = new();
}

public class SliderWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue && parameter is FrameworkElement element)
        {
            var slider = element as System.Windows.Controls.Slider;
            if (slider != null)
            {
                var percentage = (doubleValue - slider.Minimum) / (slider.Maximum - slider.Minimum);
                return slider.ActualWidth * percentage;
            }
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}

public class InvertBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true; // Default to enabled if not a boolean
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue)
        {
            return enumValue.ToString();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && targetType.IsEnum)
        {
            return Enum.Parse(targetType, stringValue);
        }
        return System.Windows.Data.Binding.DoNothing;
    }
}

public class DoubleToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue.ToString("F2");
        }
        return "0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && double.TryParse(stringValue, out double result))
        {
            return result;
        }
        return 0.0;
    }
}

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Enum enumValue && parameter is string paramString)
        {
            // Check if the enum value matches the parameter
            bool isMatch = enumValue.ToString().Equals(paramString, StringComparison.OrdinalIgnoreCase);
            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}