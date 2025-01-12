namespace WpfInstallationWizard.ViewModels
{
  public class LicenseAgreementPageViewModel : WizardPageViewModelBase
  {
    public override string Title => "License Agreement";
    public LicenseAgreementPageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}