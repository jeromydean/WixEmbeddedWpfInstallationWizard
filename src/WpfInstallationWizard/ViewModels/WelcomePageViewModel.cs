namespace WpfInstallationWizard.ViewModels
{
  public class WelcomePageViewModel : WizardPageViewModelBase
  {
    public override string Title => "Welcome";
    public WelcomePageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}