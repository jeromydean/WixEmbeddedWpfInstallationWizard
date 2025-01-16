using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WixToolset.Dtf.WindowsInstaller;
using WpfInstallationWizard.Extensions;
using WpfInstallationWizard.Messages;

namespace WpfInstallationWizard.ViewModels
{
  public partial class WizardViewModel : ObservableObject, IWizardViewModel
  {
    public ISessionProxy InstallSessionProxy { get; private set; }
    private readonly string _resourcePath;
    private readonly ManualResetEvent _installationStarted;
    private readonly ManualResetEvent _installationExited;
    private readonly IDialogCoordinator _dialogCoordinator;
    private readonly IMessenger _messenger;

    private bool _installationPerformed;
    private readonly ManualResetEvent _installationFinished;
    private readonly ManualResetEvent _installationCancelled;

    private List<IWizardPageViewModel> _wizardPages = null;
    private IWizardPageViewModel _currentPage;
    private IWizardPageViewModel _installCancelledPage;
    private IWizardPageViewModel _installFinishedPage;

    private int _currentPageIndex = 0;

    private bool _canMovePrevious = false;
    private bool _canMoveNext = false;

    private bool _isInstalling = false;

    private string _installerMessages = string.Empty;

    public IRelayCommand PreviousPageCommand { get; private set; }
    public IRelayCommand NextPageCommand { get; private set; }
    public IAsyncRelayCommand StartInstallationCommand { get; private set; }
    public IAsyncRelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
    public IRelayCommand<Window> ExitCommand { get; private set; }

    private readonly string _cancellationMutexName;
    private Mutex _installCancellationMutex = null;

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

    public WizardViewModel(ISessionProxy sessionProxy,
      string resourcePath,
      ManualResetEvent installationStarted,
      ManualResetEvent installationExited,
      IDialogCoordinator dialogCoordinator,
      IMessenger messenger)
    {
      InstallSessionProxy = sessionProxy;
      _resourcePath = resourcePath;
      _installationStarted = installationStarted;
      _installationExited = installationExited;

      _dialogCoordinator = dialogCoordinator;
      _messenger = messenger;

      _cancellationMutexName = sessionProxy["EMBEDDEDUICANCELLATIONMUTEXNAME"];
      _installationFinished = new ManualResetEvent(false);
      _installationCancelled = new ManualResetEvent(false);

      messenger.Register<InstallerMessage>(this, InstallerMessageHandler);

      ExitCommand = new RelayCommand<Window>((w) =>
      {
        w.Close();
      });

      ClosingCommand = new AsyncRelayCommand<CancelEventArgs>(VerifyClosing);

      PreviousPageCommand = new RelayCommand(() =>
      {
        if (_currentPageIndex - 1 >= 0)
        {
          _currentPageIndex--;
          CurrentPage = _wizardPages[_currentPageIndex];

          CanMovePrevious = _currentPageIndex - 1 >= 0;
          CanMoveNext = _currentPageIndex + 1 < _wizardPages.Count;
        }
      });

      NextPageCommand = new RelayCommand(() =>
      {
        if (_currentPageIndex + 1 < _wizardPages.Count)
        {
          _currentPageIndex++;
          CurrentPage = _wizardPages[_currentPageIndex];

          CanMovePrevious = _currentPageIndex - 1 >= 0;
          CanMoveNext = _currentPageIndex + 1 < _wizardPages.Count;
        }
      });

      StartInstallationCommand = new AsyncRelayCommand(async () =>
      {
        ProgressDialogController progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "title", "message", true);
        progressDialogController.SetIndeterminate();

        void ProgressDialogCancelled(object s, EventArgs ea)
        {
          progressDialogController.SetMessage("Cancelling");
          _installCancellationMutex = new Mutex(true, _cancellationMutexName);
          _installationCancelled.Set();
        };
        progressDialogController.Canceled += ProgressDialogCancelled;

        _installationStarted.Set();

        Task installCancelledTask = _installationCancelled.WaitAsync(CancellationToken.None);
        Task installFinishedTask = _installationFinished.WaitAsync(CancellationToken.None);

        Task completedTask = await Task.WhenAny(installCancelledTask, installFinishedTask);

        bool installFinished = installFinishedTask == completedTask;

        await installFinishedTask;

        progressDialogController.Canceled -= ProgressDialogCancelled;
        await progressDialogController.CloseAsync();
        _installCancellationMutex?.Dispose();
        _installationPerformed = true;
        CurrentPage = installFinished ? _installFinishedPage : _installCancelledPage;
      });
    }

    public void InstallerMessageHandler(object sender, InstallerMessage message)
    {
      //TODO process all installer messages
      //https://learn.microsoft.com/en-us/windows/win32/msi/parsing-windows-installer-messages
      if (message.MessageType == InstallMessage.InstallEnd)
      {
        _installationFinished.Set();
      }
    }

    public async Task VerifyClosing(CancelEventArgs cea)
    {
      if (!_installationPerformed)
      {
        cea.Cancel = true;
        MessageDialogResult result = await _dialogCoordinator.ShowMessageAsync(this, "Winners never quit and quitters never win", "Are you sure you would like to quit?", MessageDialogStyle.AffirmativeAndNegative);
        if (result == MessageDialogResult.Affirmative)
        {
          _installationExited.Set();
          Application.Current.Shutdown();
        }
        else
        {
          return;
        }
      }
      Application.Current.Shutdown();
    }

    public void SetPages(IWizardPageViewModel installCancelledPage, IWizardPageViewModel installFinishedPage, params IWizardPageViewModel[] installPages)
    {
      _installCancelledPage = installCancelledPage;
      _installFinishedPage = installFinishedPage;

      _wizardPages = new List<IWizardPageViewModel>(installPages);
      _currentPage = installPages.FirstOrDefault();

      CanMoveNext = _wizardPages.Count >= 2;
      CanMovePrevious = false;
    }
  }
}