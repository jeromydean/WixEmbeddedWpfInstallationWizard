using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard
{
  public class WpfInstallationWizardEmbeddedUI : IEmbeddedUI
  {
    private Session _session;
    private string _resourcePath;

    private Thread _applicationThread;
    private ManualResetEvent _installerStartedEvent;
    private ManualResetEvent _installerExitedEvent;
    private WpfInstallationWizardApplication _installationWizardApplication;

    public bool Initialize(Session session, string resourcePath, ref InstallUIOptions internalUILevel)
    {
      _session = session;
      _resourcePath = resourcePath;

      _installerStartedEvent = new ManualResetEvent(false);
      _installerExitedEvent = new ManualResetEvent(false);

      _applicationThread = new Thread(StartWpfInstallationWizardApplication);
      _applicationThread.SetApartmentState(ApartmentState.STA);
      _applicationThread.Start();

      int waitAny = WaitHandle.WaitAny(new WaitHandle[] { _installerStartedEvent, _installerExitedEvent });

      //did the user exit the installation?
      if (waitAny == 1)
      {
        throw new InstallCanceledException();
      }
      return true;
    }

    private void StartWpfInstallationWizardApplication()
    {
      _installationWizardApplication = new WpfInstallationWizardApplication(_session,
        _resourcePath,
        _installerStartedEvent,
        _installerExitedEvent);
      _installationWizardApplication.Start();
    }

    public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
    {
      return MessageResult.OK;
    }

    public void Shutdown()
    {
      _applicationThread.Join();
    }
  }
}
