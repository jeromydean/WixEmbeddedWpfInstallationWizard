using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for LicenseAgreementPage.xaml
  /// </summary>
  public partial class LicenseAgreementPage : WizardPageBase
  {
    public LicenseAgreementPage(LicenseAgreementPageViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}