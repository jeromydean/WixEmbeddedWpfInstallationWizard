using System;
using System.Threading;
using System.Threading.Tasks;

namespace WpfInstallationWizard.Extensions
{
  public static class WaitHandleExtensions
  {
    public static async Task WaitAsync(this WaitHandle waitHandle, CancellationToken cancellationToken)
    {
      if (waitHandle == null)
      {
        throw new ArgumentNullException(nameof(waitHandle));
      }

      TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

      using (cancellationToken.Register(() => taskCompletionSource.TrySetCanceled()))
      {
        ThreadPool.QueueUserWorkItem(_ =>
        {
          if (!cancellationToken.IsCancellationRequested && waitHandle.WaitOne())
          {
            taskCompletionSource.SetResult(true);
          }
        });

        try
        {
          await taskCompletionSource.Task;
        }
        catch (OperationCanceledException)
        {
          throw new OperationCanceledException("The wait operation was canceled.");
        }
      }
    }
  }
}
