using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfInstallationWizard.ValueConverters
{
  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class InvertableBooleanToVisibilityConverter : IValueConverter
  {
    enum Direction
    {
      Normal,
      Inverted
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool boolValue = (bool)value;
      Direction direction = parameter != null
        ? (Direction)Enum.Parse(typeof(Direction), (string)parameter)
        : Direction.Normal;

      if (direction == Direction.Inverted)
      {
        return !boolValue ? Visibility.Visible : Visibility.Collapsed;
      }
        
      return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }
  }
}