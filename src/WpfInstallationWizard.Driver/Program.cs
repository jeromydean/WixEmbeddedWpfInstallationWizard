using System.Threading.Tasks;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.Tester
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      try
      {
        WpfEmbeddedUI wpfEmbeddedUI = new WpfEmbeddedUI();
        InstallUIOptions installUIOptions = InstallUIOptions.Full;
        wpfEmbeddedUI.Initialize(new SessionProxy(), string.Empty, ref installUIOptions);

        await Task.Delay(10000);

        wpfEmbeddedUI.ProcessMessage(InstallMessage.Info, new Record("Action ended"), MessageButtons.OK, MessageIcon.Information, MessageDefaultButton.Button1);
        wpfEmbeddedUI.Shutdown();
      }
      catch (InstallCanceledException) { }      
    }
  }
}