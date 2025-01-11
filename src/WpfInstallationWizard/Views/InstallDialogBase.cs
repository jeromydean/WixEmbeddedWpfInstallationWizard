using MahApps.Metro.Controls;

namespace WpfInstallationWizard.Views
{
  public abstract class InstallDialogBase : MetroWindow
  {
    public InstallDialogBase()
    {
      TitleCharacterCasing = System.Windows.Controls.CharacterCasing.Normal;
    }
  }
}
