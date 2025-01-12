using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfInstallationWizard.ViewModels
{
  public abstract class WizardPageViewModelBase : ObservableObject, IWizardPageViewModel
  {
    private bool _previousPageEnabled = false;
    private bool _nextPageEnabled = false;

    public bool PreviousPageEnabled
    {
      get
      {
        return _previousPageEnabled;
      }
      set
      {
        SetProperty(ref _nextPageEnabled, value);
      }
    }

    public bool NextPageEnabled
    {
      get
      {
        return _nextPageEnabled;
      }
      set
      {
        SetProperty(ref _nextPageEnabled, value);
      }
    }

    protected IWizardViewModel WizardViewModel { get; private set; }
    public abstract string Title { get; }
    public virtual ICommand PreviousCommand { get; private set; }
    public virtual ICommand NextCommand { get; private set; }
    public virtual IRelayCommand<Window> InstallCommand { get; private set; }
    public virtual IRelayCommand<Window> CancelCommand { get; private set; }

    public WizardPageViewModelBase(IWizardViewModel wizardViewModel)
    {
      WizardViewModel = wizardViewModel;

      PreviousCommand = new RelayCommand(() =>
      {
        WizardViewModel.PreviousPage();
      }, () => { return WizardViewModel.CanMovePrevious; });

      NextCommand = new RelayCommand(() =>
      {
        WizardViewModel.NextPage();
      }, () => { return WizardViewModel.CanMoveNext; });

      InstallCommand = new RelayCommand<Window>((w) =>
      {
        w.DialogResult = true;
      });

      CancelCommand = new RelayCommand<Window>((w) =>
      {
        w.DialogResult = false;
      });
    }
  }
}