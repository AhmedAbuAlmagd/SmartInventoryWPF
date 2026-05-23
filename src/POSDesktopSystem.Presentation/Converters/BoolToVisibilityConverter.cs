using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace POSDesktopSystem.Presentation.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool IsInverted { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool bValue = false;
        if (value is bool b) bValue = b;
        else if (value is int i) bValue = i > 0;
        else if (value is long l) bValue = l > 0;
        else if (value != null) bValue = true;

        if (IsInverted) bValue = !bValue;
        return bValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
