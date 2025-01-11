using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WixToolset.Dtf.WindowsInstaller;
using WpfInstallationWizard.Services;
using WpfInstallationWizard.ViewModels;
using WpfInstallationWizard.Views;

namespace WpfInstallationWizard
{
  public class WpfInstallationWizardApplication : Application
  {
    private IServiceProvider _serviceProvider;
    private IInstallationService _installationService;

    private readonly Session _session;
    private readonly string _resourcePath;
    private readonly ManualResetEvent _installerStartedEvent;
    private readonly ManualResetEvent _installerExitedEvent;

    public WpfInstallationWizardApplication(Session session,
      string resourcePath,
      ManualResetEvent installerStartedEvent,
      ManualResetEvent installerExitedEvent)
    {
      _session = session;
      _resourcePath = resourcePath;
      _installerStartedEvent = installerStartedEvent;
      _installerExitedEvent = installerExitedEvent;

      ShutdownMode = ShutdownMode.OnExplicitShutdown;
    }

    public void Start()
    {
      Run();
    }
    protected override void OnStartup(StartupEventArgs e)
    {
      Debugger.Launch();

      ServiceCollection serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      _serviceProvider = serviceCollection.BuildServiceProvider();

      _installationService = _serviceProvider.GetRequiredService<IInstallationService>();

      //add theming support to the app resources
      Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(@"pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml")
      });
      Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(@"pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml")
      });
      //color schemes ("Light" and "Dark" variations of all of these exist):
      //"Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cyan", "Cobalt", "Indigo",
      //"Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna"
      Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
      {
        Source = new Uri(@"pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Blue.xaml")
      });

      base.OnStartup(e);

      WelcomeDialog welcomeDialog = _serviceProvider.GetRequiredService<WelcomeDialog>();
      welcomeDialog.ShowDialog();

      _installerExitedEvent.Set();
      Application.Current.Shutdown();
    }

    public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
    {
      using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
      {
        sw.WriteLine($"{DateTime.Now}: ProcessMessage invoked, messageType={messageType}, messageRecord={messageRecord}, buttons={buttons}, icon={icon}, defaultButton={defaultButton}");
      }

      if (_installationService != null)
      {
        object messageResult = Dispatcher.Invoke(DispatcherPriority.Send,
          new Func<MessageResult>(delegate ()
          {
            return _installationService.ProcessMessage(messageType, messageRecord, buttons, icon, defaultButton);
          }));
        return (MessageResult)messageResult;
      }
      return MessageResult.Cancel;
    }

    private void ConfigureServices(IServiceCollection services)
    {
      services.AddSingleton<IInstallationService>(new InstallationService(_session, _resourcePath, _installerStartedEvent, _installerExitedEvent));
      
      //register all dialog view models
      Type viewModelBaseType = typeof(InstallDialogViewModelBase);
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      foreach (Type viewModelType in executingAssembly.GetTypes().Where(t => t != viewModelBaseType
        && viewModelBaseType.IsAssignableFrom(t)))
      {
        services.AddSingleton(viewModelType);
      }

      //register all dialogs
      Type installDialogBaseType = typeof(InstallDialogBase);
      foreach (Type installDialogType in executingAssembly.GetTypes().Where(t => t != installDialogBaseType
        && installDialogBaseType.IsAssignableFrom(t)))
      {
        services.AddSingleton(installDialogType);
      }
    }
  }
}
