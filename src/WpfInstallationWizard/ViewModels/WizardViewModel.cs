using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.ViewModels
{
  public class WizardViewModel : ObservableObject, IWizardViewModel
  {
    private readonly Session _session;
    private readonly string _resourcePath;
    private readonly ManualResetEvent _installerStartedEvent;
    private readonly ManualResetEvent _installerExitedEvent;

    private List<IWizardPageViewModel> _wizardPages = null;
    private IWizardPageViewModel _currentPage;
    private int _currentPageIndex = 0;

    private bool _canMovePrevious = false;
    private bool _canMoveNext = false;

    public bool CanMovePrevious
    {
      get
      {
        return _canMovePrevious;
      }
      set
      {
        SetProperty(ref _canMovePrevious, value);
      }
    }

    public bool CanMoveNext
    {
      get
      {
        return _canMoveNext;
      }
      set
      {
        SetProperty(ref _canMoveNext, value);
      }
    }

    public IWizardPageViewModel CurrentPage
    {
      get
      {
        return _currentPage;
      }
      set
      {
        SetProperty(ref _currentPage, value);
      }
    }

    public WizardViewModel(Session session,
      string resourcePath,
      ManualResetEvent installerStartedEvent,
      ManualResetEvent installerExitedEvent)
    {
      _session = session;
      _resourcePath = resourcePath;
      _installerStartedEvent = installerStartedEvent;
      _installerExitedEvent = installerExitedEvent;
    }

    public void AddPages(params IWizardPageViewModel[] pages)
    {
      _wizardPages = new List<IWizardPageViewModel>(pages);
      _currentPage = pages.FirstOrDefault();

      CanMoveNext = _wizardPages.Count >= 2;
      CanMovePrevious = false;
    }

    public void Cancel()
    {
      throw new System.NotImplementedException();
    }

    public bool NextPage()
    {
      if (_currentPageIndex + 1 < _wizardPages.Count)
      {
        _currentPageIndex++;
        CurrentPage = _wizardPages[_currentPageIndex];
        CanMoveNext = _currentPageIndex + 1 < _wizardPages.Count;
        return true;
      }
      return false;
    }

    public bool PreviousPage()
    {
      if (_currentPageIndex - 1 >= 0)
      {
        _currentPageIndex--;
        CurrentPage = _wizardPages[_currentPageIndex];
        CanMovePrevious = _currentPageIndex - 1 >= 0;
        return true;
      }
      return false;
    }

    public void StartInstall()
    {
      throw new System.NotImplementedException();
    }
  }
}
