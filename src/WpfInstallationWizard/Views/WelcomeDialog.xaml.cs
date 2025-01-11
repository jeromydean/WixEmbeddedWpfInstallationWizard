using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for WelcomeDialog.xaml
  /// </summary>
  public partial class WelcomeDialog : InstallDialogBase
  {
    public WelcomeDialog(WelcomeDialogViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}