namespace WpfInstallationWizard.ViewModels
{
  public interface IWizardViewModel
  {
    ISessionProxy InstallSessionProxy { get; }
    void SetPages(IWizardPageViewModel installCancelledPage, IWizardPageViewModel installFinishedPage, params IWizardPageViewModel[] installPages);
  }
}