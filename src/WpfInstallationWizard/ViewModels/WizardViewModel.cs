using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
    private readonly ManualResetEvent _installationSequenceAborted;
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
    private ProgressDialogController _progressDialogController = null;

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
      ManualResetEvent installationSequenceAborted,
      ManualResetEvent installationExited,
      IDialogCoordinator dialogCoordinator,
      IMessenger messenger)
    {
      InstallSessionProxy = sessionProxy;
      _resourcePath = resourcePath;
      _installationStarted = installationStarted;
      _installationSequenceAborted = installationSequenceAborted;
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
        _progressDialogController = await _dialogCoordinator.ShowProgressAsync(this, "Installing", "Starting installation", true);
        _progressDialogController.SetIndeterminate();

        void CancelInstall()
        {
          _installCancellationMutex = new Mutex(true, _cancellationMutexName);
          _installationCancelled.Set();
          _progressDialogController.SetTitle("Cancelling installation");
          _progressDialogController.SetMessage("Please wait");
        }

        void ProgressDialogCancelled(object s, EventArgs ea)
        {
          CancelInstall();
        };
        _progressDialogController.Canceled += ProgressDialogCancelled;

        _installationStarted.Set();

        //aborted = declined UAC prompt or if something fails during the install
        Task installAbortedTask = _installationSequenceAborted.WaitAsync(CancellationToken.None);
        //cancelled = user cancel from the wizard UI
        Task installCancelledTask = _installationCancelled.WaitAsync(CancellationToken.None);
        //finished = recieved the InstallMessage.InstallEnd message through the interface
        Task installFinishedTask = _installationFinished.WaitAsync(CancellationToken.None);

        //in the case of an abort you won't get the InstallEnd message so it'll never "finish"
        Task completedTask = await Task.WhenAny(installAbortedTask, installCancelledTask, installFinishedTask);
        bool installCompletedSuccessfully = completedTask == installFinishedTask;
        if (completedTask == installCancelledTask)
        {
          await Task.WhenAny(installAbortedTask, installFinishedTask);
        }

        _progressDialogController.Canceled -= ProgressDialogCancelled;
        await _progressDialogController.CloseAsync();
        _progressDialogController = null;
        _installCancellationMutex?.Dispose();
        _installationPerformed = true;
        CurrentPage = installCompletedSuccessfully ? _installFinishedPage : _installCancelledPage;
      });
    }

    public void InstallerMessageHandler(object sender, InstallerMessage message)
    {
      ///TODO process all installer messages -- some of them will require new dialogs/pages
      //https://learn.microsoft.com/en-us/windows/win32/msi/parsing-windows-installer-messages
      switch (message.MessageType)
      {
        case InstallMessage.Info:
          if (message.MessageRecord != null
            && message.MessageRecord.FieldCount >= 2
            && !string.IsNullOrEmpty(message.MessageRecord[2].ToString()))
          {
            _progressDialogController?.SetMessage(message.MessageRecord[2].ToString());
          }
          break;
        case InstallMessage.ActionStart:
          //MessageRecord can be null
          //MessageRecord.Fields is 1 based
          if (message.MessageRecord != null)
          {
            string actionName = message.MessageRecord.FieldCount >= 2 ? message.MessageRecord[2].ToString() : string.Empty;
            string actionDescription = message.MessageRecord.FieldCount >= 3 ? message.MessageRecord[3].ToString() : string.Empty;
            string formattedString = message.MessageRecord.FormatString.Replace("[1]", actionName)
              .Replace("[2]", actionDescription).Trim();

            _progressDialogController?.SetMessage(formattedString);
          }
          break;
        case InstallMessage.InstallEnd:
          _installationFinished.Set();
          break;
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
          await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
          {
            Application.Current.Shutdown();
          }), DispatcherPriority.Normal);
          return;
        }
        else
        {
          return;
        }
      }
      await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
      {
        Application.Current.Shutdown();
      }), DispatcherPriority.Normal);
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