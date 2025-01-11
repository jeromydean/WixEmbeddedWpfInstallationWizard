using WpfInstallationWizard.Services;

namespace WpfInstallationWizard.ViewModels
{
  public class WelcomeDialogViewModel : InstallDialogViewModelBase
  {
    private readonly IInstallationService _installationService;
    public WelcomeDialogViewModel(IInstallationService installationService)
    {
      _installationService = installationService;
    }
  }
}
