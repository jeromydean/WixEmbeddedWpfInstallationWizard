using System;
using System.Runtime.Serialization;
using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.InterfaceTester
{
  public class App
  {
    private static Thread _applicationThread;
    private static Session _session;
    private static ManualResetEvent _installerStartedEvent;
    private static ManualResetEvent _installerExitedEvent;
    private static WpfInstallationWizardApplication _installationWizardApplication;

    [STAThread]
    public static void Main()
    {
      _session = (Session)FormatterServices.GetUninitializedObject(typeof(Session));
      _installerStartedEvent = new ManualResetEvent(false);
      _installerExitedEvent = new ManualResetEvent(false);

      _applicationThread = new Thread(StartWpfInstallationWizardApplication);
      _applicationThread.SetApartmentState(ApartmentState.STA);
      _applicationThread.Start();

      int waitAny = WaitHandle.WaitAny(new WaitHandle[] { _installerStartedEvent, _installerExitedEvent });
    }

    private static void StartWpfInstallationWizardApplication()
    {
      _installationWizardApplication = new WpfInstallationWizardApplication(_session,
        null,
        _installerStartedEvent,
        _installerExitedEvent);
      _installationWizardApplication.Start();
    }
  }
}
