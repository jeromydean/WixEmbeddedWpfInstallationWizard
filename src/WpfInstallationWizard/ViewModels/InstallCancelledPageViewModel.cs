namespace WpfInstallationWizard.ViewModels
{
  public class InstallCancelledPageViewModel : WizardPageViewModelBase
  {
    public override string Title => "Install Cancelled";
    public InstallCancelledPageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}
