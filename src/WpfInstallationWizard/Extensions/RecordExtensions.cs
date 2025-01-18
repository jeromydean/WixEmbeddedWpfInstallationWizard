using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard.Extensions
{
  internal static class RecordExtensions
  {
    public static string Parse(this Record record)
    {
      if (record != null)
      {
        string formatString;
        if (!string.IsNullOrEmpty((formatString = record.FormatString)))
        {
          for(int f = 1;f <= record.FieldCount; f++)
          {
            formatString = formatString.Replace($"[{f}]", record[f]?.ToString() ?? string.Empty);
          }
          return formatString;
        }
      }
      return null;
    }
  }
}