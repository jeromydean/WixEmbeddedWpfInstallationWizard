using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for WelcomePage.xaml
  /// </summary>
  public partial class WelcomePage : WizardPageBase
  {
    public WelcomePage(WelcomePageViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}
