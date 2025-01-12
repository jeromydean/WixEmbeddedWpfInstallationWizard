using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using WpfInstallationWizard.Views;

namespace WpfInstallationWizard.ValueConverters
{
  public class WizardPageDataContextToWizardPageConverter : IValueConverter
  {
    private IServiceProvider _serviceProvider;

    public WizardPageDataContextToWizardPageConverter(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string pageTypeName = $"{typeof(IWizardPage).Namespace}.{value.GetType().Name.Replace("ViewModel", string.Empty)}";
      Type pageType = Type.GetType(pageTypeName, false);

      if (pageType != null)
      {
        return _serviceProvider.GetRequiredService(pageType);
      }
      else
      {
        return new TextBlock { Text = "Page not found '{}'." };
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}