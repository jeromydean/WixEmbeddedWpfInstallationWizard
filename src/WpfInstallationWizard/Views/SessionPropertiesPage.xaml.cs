using WpfInstallationWizard.ViewModels;

namespace WpfInstallationWizard.Views
{
  /// <summary>
  /// Interaction logic for SessionPropertiesPage.xaml
  /// </summary>
  public partial class SessionPropertiesPage : WizardPageBase
  {
    public SessionPropertiesPage(SessionPropertiesPageViewModel dataContext)
    {
      DataContext = dataContext;
      InitializeComponent();
    }
  }
}