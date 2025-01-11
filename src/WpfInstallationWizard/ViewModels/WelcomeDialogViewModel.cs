using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WpfInstallationWizard.Services;

namespace WpfInstallationWizard.ViewModels
{
  public class WelcomeDialogViewModel : InstallDialogViewModelBase
  {
    private readonly IInstallationService _installationService;

    public ICommand PreviousCommand { get; private set; }
    public ICommand NextCommand { get; private set; }
    public ICommand InstallCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }

    public WelcomeDialogViewModel(IInstallationService installationService)
    {
      _installationService = installationService;

      NextCommand = new RelayCommand(() =>
      {
        _installationService.MoveNext();
      });

      PreviousCommand = new RelayCommand(() =>
      {
        _installationService.MovePrevious();
      });

      CancelCommand = new RelayCommand(() =>
      {
        _installationService.Cancel();
      });

      InstallCommand = new RelayCommand(() =>
      {
        _installationService.PerformInstall();
      });
    }
  }
}
