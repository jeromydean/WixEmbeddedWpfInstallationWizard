namespace WpfInstallationWizard.ViewModels
{
  public class SessionPropertiesPageViewModel : WizardPageViewModelBase
  {
    public override string Title => "Session Properties";
    public SessionPropertiesPageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}