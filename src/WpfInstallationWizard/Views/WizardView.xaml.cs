using MahApps.Metro.Controls;
using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for WizardView.xaml
  /// </summary>
  public partial class WizardView : MetroWindow
  {
    public WizardView(IWizardViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}