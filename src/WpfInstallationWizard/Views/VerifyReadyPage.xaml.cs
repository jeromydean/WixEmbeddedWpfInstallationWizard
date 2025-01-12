using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for VerifyReadyPage.xaml
  /// </summary>
  public partial class VerifyReadyPage : WizardPageBase
  {
    public VerifyReadyPage(VerifyReadyPageViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}
