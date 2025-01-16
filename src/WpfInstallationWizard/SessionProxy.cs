using System.Collections.Generic;
using WixToolset.Dtf.WindowsInstaller;

namespace WpfInstallationWizard
{
  public class SessionProxy : ISessionProxy
  {
    private Session _session;
    private readonly Dictionary<string, string> _properties;

    public SessionProxy(Session session = null)
    {
      _session = session;
      _properties = new Dictionary<string, string>();
    }

    public string this[string property]
    {
      get
      {
        return _session != null
          ? _session[property]
          : _properties[property];
      }
      set
      {
        if (_session != null)
        {
          _session[property] = value;
        }
        else
        {
          _properties[property] = value;
        }
      }
    }
  }
}
