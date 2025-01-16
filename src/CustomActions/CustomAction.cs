using System.Threading;
using WixToolset.Dtf.WindowsInstaller;

namespace CustomActions
{
  public class CustomActions
  {
    [CustomAction]
    public static ActionResult PerformCustomAction(Session session)
    {
      session.Log("Executed PerformCustomAction");

      return ActionResult.Success;
    }

    [CustomAction]
    public static ActionResult CheckEmbeddedUICancellation(Session session)
    {
      string cancellationMutexName = null;
      if (!string.IsNullOrEmpty((cancellationMutexName = session.CustomActionData["EMBEDDEDUICANCELLATIONMUTEXNAME"])))
      {
        using (Mutex cancellationMutex = new Mutex(true, cancellationMutexName, out bool newMutex))
        {
          if (!newMutex)
          {
            session.Log("Installation cancelled.");
            return ActionResult.UserExit;
          }
        }
      }

      return ActionResult.Success;
    }
  }
}