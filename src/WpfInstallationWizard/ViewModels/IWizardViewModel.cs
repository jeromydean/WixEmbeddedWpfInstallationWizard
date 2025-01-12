namespace WpfInstallationWizard.ViewModels
{
  public interface IWizardViewModel
  {
    void AddPages(params IWizardPageViewModel[] pages);
    bool PreviousPage();
    bool NextPage();
    void Cancel();
    void StartInstall();
    bool CanMovePrevious { get; }
    bool CanMoveNext { get; }
  }
}
