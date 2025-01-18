using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for InstallFinishedPage.xaml
  /// </summary>
  public partial class InstallFinishedPage : WizardPageBase
  {
    public InstallFinishedPage(InstallFinishedPageViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}
