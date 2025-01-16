using System.Collections.Generic;
using System.Linq;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.Extensions
{
  public static class SessionExtensions
  {
    //https://learn.microsoft.com/en-us/windows/win32/msi/sql-syntax
    public static IEnumerable<string> GetPropertyNames(this Session session)
    {
      using (View view = session.Database.OpenView("SELECT `Property` FROM `Property`"))
      {
        view.Execute();
        foreach(string property in view.Select(r => r.GetString("Property")))
        {
          yield return property;
        }
      }
    }
    public static IEnumerable<string> GetPropertyValues(this Session session)
    {
      using (View view = session.Database.OpenView("SELECT `Value` FROM `Property`"))
      {
        view.Execute();
        foreach (string property in view.Select(r => r.GetString("Value")))
        {
          yield return property;
        }
      }
    }
  }
}