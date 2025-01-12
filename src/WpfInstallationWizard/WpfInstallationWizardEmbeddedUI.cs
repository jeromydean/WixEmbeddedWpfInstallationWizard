using System;
using System.IO;
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
      //https://learn.microsoft.com/en-us/windows/win32/msi/using-an-embedded-ui

      using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      {
        sw.WriteLine($"{DateTime.Now}: Initialize invoked, session={session}, resourcePath={resourcePath}, internalUILevel={internalUILevel}");
      }

      _session = session;
      _resourcePath = resourcePath;

      _installerStartedEvent = new ManualResetEvent(false);
      _installerExitedEvent = new ManualResetEvent(false);

      _applicationThread = new Thread(StartWpfInstallationWizardApplication);
      _applicationThread.SetApartmentState(ApartmentState.STA);
      _applicationThread.Start();

      int waitAny = WaitHandle.WaitAny(new WaitHandle[] { _installerStartedEvent, _installerExitedEvent });

      using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      {
        sw.WriteLine($"{DateTime.Now}: WaitHandle.WaitAny, result={waitAny}");
      }

      //did the user exit the installation?
      if (waitAny == 1)
      {
        throw new InstallCanceledException();
      }

      using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      {
        sw.WriteLine($"{DateTime.Now}: Starting installation");
      }

      internalUILevel = InstallUIOptions.NoChange | InstallUIOptions.SourceResolutionOnly;
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
      return _installationWizardApplication.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
    }

    public void Shutdown()
    {
      using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      {
        sw.WriteLine($"{DateTime.Now}: Shutdown invoked");
      }
      _applicationThread.Join();
    }
  }
}
