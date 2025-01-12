namespace WpfInstallationWizard.ViewModels
{
  public class VerifyReadyPageViewModel : WizardPageViewModelBase
  {
    public override string Title => "Ready To Install";
    public VerifyReadyPageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}