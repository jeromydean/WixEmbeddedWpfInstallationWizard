using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WixToolset.Dtf.WindowsInstaller;
using WpfInstallationWizard.Messages;

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

    private bool _isInstalling = false;

    private string _installerMessages = string.Empty;

    public virtual IRelayCommand<Window> ExitCommand { get; private set; }

    public bool IsInstalling
    {
      get
      {
        return _isInstalling;
      }
      set
      {
        SetProperty(ref _isInstalling, value);
      }
    }

    public string InstallerMessages
    {
      get
      {
        return _installerMessages;
      }
      set
      {
        SetProperty(ref _installerMessages, value);
      }
    }

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

      //WeakReferenceMessenger.Default.Register<InstallerMessage>(this, (r, m) =>
      //{
      //  _installerMessages = $"{_installerMessages}\r\n{$"{DateTime.Now}: ProcessMessage received in WizardViewModel, messageType={m.MessageType}, messageRecord={m.MessageRecord}"}";

      //  using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      //  {
      //    sw.WriteLine($"{DateTime.Now}: ProcessMessage received in WizardViewModel, messageType={m.MessageType}, messageRecord={m.MessageRecord}");
      //  }
      //});

      ExitCommand = new RelayCommand<Window>((w) =>
      {
        w.DialogResult = true;
      });
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
      IsInstalling = true;

      //would be nice to pop a progress dialog up here...  gotta do the messenger stuff and listen for the records
      _installerStartedEvent.Set();
    }

    public void ProcessInstallerMessage(InstallerMessage m)
    {
      InstallerMessages = $"{_installerMessages}{Environment.NewLine}{$"{DateTime.Now}: ProcessMessage received in WizardViewModel, messageType={m.MessageType}, messageRecord={m.MessageRecord}"}";

      using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      {
        sw.WriteLine($"{DateTime.Now}: ProcessMessage received in WizardViewModel, messageType={m.MessageType}, messageRecord={m.MessageRecord}");
      }
    }
  }
}
