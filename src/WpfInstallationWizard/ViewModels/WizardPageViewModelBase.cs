using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfInstallationWizard.ViewModels
{
  public abstract class WizardPageViewModelBase : ObservableObject, IWizardPageViewModel
  {
    protected IWizardViewModel WizardViewModel { get; private set; }
    public abstract string Title { get; }

    public WizardPageViewModelBase(IWizardViewModel wizardViewModel)
    {
      WizardViewModel = wizardViewModel;
    }
  }
}