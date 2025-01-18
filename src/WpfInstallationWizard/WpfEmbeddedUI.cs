using System;
using System.Threading;
using WixToolset.Dtf.WindowsInstaller;
using WpfInstallationWizard.Messages;

namespace WpfInstallationWizard
{
  public class WpfEmbeddedUI : IEmbeddedUI
  {
    private Thread _applicationThread;
    private WpfEmbeddedUIApplication _wpfEmbeddedApplication;
    private ISessionProxy _sessionProxy;
    private string _resourcePath;
    private ManualResetEvent _installationStarted;
    private ManualResetEvent _installationExited;

    public bool Initialize(Session session, string resourcePath, ref InstallUIOptions internalUILevel)
    {
      return Initialize(new SessionProxy(session), resourcePath, ref internalUILevel);
    }

    public bool Initialize(ISessionProxy sessionProxy, string resourcePath, ref InstallUIOptions internalUILevel)
    {
      _sessionProxy = sessionProxy;
      _resourcePath = resourcePath;

      _sessionProxy["EMBEDDEDUICANCELLATIONMUTEXNAME"] = Guid.NewGuid().ToString();

      using (_installationStarted = new ManualResetEvent(false))
      {
        using (_installationExited = new ManualResetEvent(false))
        {
          _applicationThread = new Thread(StartWpfEmbeddedUIApplication);
          _applicationThread.SetApartmentState(ApartmentState.STA);
          _applicationThread.Start();

          int waitHandleResult = WaitHandle.WaitAny(new WaitHandle[] { _installationStarted, _installationExited });
          if (waitHandleResult == 1)
          {
            throw new InstallCanceledException();
          }

          //https://learn.microsoft.com/en-us/windows/win32/msi/using-an-embedded-ui
          ///TODO make sure we can actually handle the UI actions that are being requested
          internalUILevel = InstallUIOptions.NoChange | InstallUIOptions.SourceResolutionOnly;
          return true;
        }
      }
    }

    private void StartWpfEmbeddedUIApplication()
    {
      _wpfEmbeddedApplication = new WpfEmbeddedUIApplication(_sessionProxy,
        _resourcePath,
        _installationStarted,
        _installationExited);
      _wpfEmbeddedApplication.Start();
    }

    public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
    {
      //we ignore these message types so just drop them and return OK
      if (messageType == InstallMessage.InstallStart
        || messageType == InstallMessage.ActionData
        || messageType == InstallMessage.CommonData
        || messageType == InstallMessage.Info
        || messageType == InstallMessage.Progress)
      {
        return MessageResult.OK;
      }

      return _wpfEmbeddedApplication.ProcessMessage(messageType,
        messageRecord,
        buttons,
        icon,
        defaultButton);
    }

    /// <summary>
    // ShutdownEmbeddedUI is optional. It can allow the embedded UI to perform any cleanup. After this call, the embedded UI should not receive any additional callbacks.
    /// </summary>
    public void Shutdown()
    {
      _applicationThread.Join();
    }
  }
}