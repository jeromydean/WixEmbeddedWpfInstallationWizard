namespace WpfInstallationWizard.ViewModels
{
  public class LicenseAgreementPageViewModel : WizardPageViewModelBase
  {
    private bool _licenseAccepted;

    public override string Title => "License Agreement";

    public bool LicenseAccepted
    {
      get
      {
        return _licenseAccepted;
      }
      set
      {
        SetProperty(ref _licenseAccepted, value);
      }
    }
    public LicenseAgreementPageViewModel(IWizardViewModel wizardViewModel) : base(wizardViewModel)
    {
    }
  }
}