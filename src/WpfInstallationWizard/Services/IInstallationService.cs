using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.Services
{
  public interface IInstallationService
  {
    void MoveNext();
    void MovePrevious();
    void Cancel();
    void PerformInstall();
    MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton);
  }
}
