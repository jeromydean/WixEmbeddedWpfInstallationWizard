using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for InstallCancelledPage.xaml
  /// </summary>
  public partial class InstallCancelledPage : WizardPageBase
  {
    public InstallCancelledPage(InstallCancelledPageViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}
