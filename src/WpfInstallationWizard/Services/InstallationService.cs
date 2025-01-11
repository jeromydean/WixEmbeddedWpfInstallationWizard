using System.Threading;
using System.Windows;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.Services
{
  public class InstallationService : IInstallationService
  {
    private readonly Session _session;
    private readonly string _resourcePath;
    private readonly ManualResetEvent _installerStartedEvent;
    private readonly ManualResetEvent _installerExitedEvent;

    public InstallationService(Session session,
      string resourcePath,
      ManualResetEvent installerStartedEvent,
      ManualResetEvent installerExitedEvent)
    {
      _session = session;
      _resourcePath = resourcePath;
      _installerStartedEvent = installerStartedEvent;
      _installerExitedEvent = installerExitedEvent;
    }

    public void Cancel()
    {
      _installerExitedEvent.Set();

      //Application.Current.Shutdown();
    }

    public void MoveNext()
    {
      throw new System.NotImplementedException();
    }

    public void MovePrevious()
    {
      throw new System.NotImplementedException();
    }

    public void PerformInstall()
    {
      _installerStartedEvent.Set();
    }

    public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
    {
      return MessageResult.OK;
    }
  }
}
