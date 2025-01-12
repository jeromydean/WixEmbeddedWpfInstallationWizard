using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.Messages
{
  public class InstallerMessage
  {
    public InstallMessage MessageType { get; set; }
    public Record MessageRecord { get; set; }
  }
}