using System;
using System.IO;
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

        for(int i = 0; i < 5; i++)
        {
          wpfEmbeddedUI.ProcessMessage(InstallMessage.ActionStart, new Record(string.Empty, (i + 1).ToString(), "5")
          {
            FormatString = $"Install Wizard Driver, step [1] of [2]"
          }, MessageButtons.OK, MessageIcon.Information, MessageDefaultButton.Button1);
          await Task.Delay(250);
        }

        await Task.Delay(1000);

        wpfEmbeddedUI.ProcessMessage(InstallMessage.InstallEnd, new Record(), MessageButtons.OK, MessageIcon.Information, MessageDefaultButton.Button1);
        wpfEmbeddedUI.Shutdown();
      }
      catch (InstallCanceledException) { }
      catch(Exception ex)
      {
        Console.WriteLine($"An error occurred.  {ex}");
      }
      Console.WriteLine("Finished");
      Console.ReadLine();
    }
  }
}