namespace WpfInstallationWizard.ViewModels
{
  public class InstallFinishedPageViewModel : WizardPageViewModelBase
  {
    public override string Title => "Install Finished";
    public InstallFinishedPageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}
