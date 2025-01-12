using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using WixToolset.Dtf.WindowsInstaller;
using WpfInstallationWizard.Messages;
using WpfInstallationWizard.ValueConverters;
using WpfInstallationWizard.ViewModels;
using WpfInstallationWizard.Views;

namespace WpfInstallationWizard
{
  public class WpfInstallationWizardApplication : Application
  {
    private IServiceProvider _serviceProvider;

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

      //we are being explicit even though this is the default
      ShutdownMode = ShutdownMode.OnLastWindowClose;
    }

    public void Start()
    {
      Run();
    }
    protected override void OnStartup(StartupEventArgs e)
    {
      try
      {
        ServiceCollection serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        _serviceProvider = serviceCollection.BuildServiceProvider();

        Application.Current.Resources.Add(nameof(WizardPageDataContextToWizardPageConverter), _serviceProvider.GetRequiredService<WizardPageDataContextToWizardPageConverter>());

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

        _serviceProvider.GetRequiredService<IWizardViewModel>().AddPages(
          _serviceProvider.GetRequiredService<WelcomePageViewModel>(),
          _serviceProvider.GetRequiredService<LicenseAgreementPageViewModel>(),
          _serviceProvider.GetRequiredService<VerifyReadyPageViewModel>());

        WizardView wizardView = _serviceProvider.GetRequiredService<WizardView>();
        if (wizardView.ShowDialog() == false)
        {
          _installerExitedEvent.Set();
        }
      }
      catch (Exception ex)
      {
        using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
        {
          sw.WriteLine($"{DateTime.Now}: OnStartup Exception occurred. {ex}");
        }
      }
      finally
      {
        Application.Current.Shutdown();
      }
    }

    //public void PerformShutdown()
    //{
    //  _serviceProvider.GetRequiredService<WizardView>().DialogResult = null;
    //}

    public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
    {
      try
      {
        _serviceProvider.GetRequiredService<IWizardViewModel>().ProcessInstallerMessage(new InstallerMessage
        {
          MessageType = messageType,
          MessageRecord = messageRecord
        });

        //gotta figure out why this throws
        //System.IO.FileLoadException: Could not load file or assembly 'System.Runtime.CompilerServices.Unsafe, Version=6.0.0.0, 
        //WeakReferenceMessenger.Default.Send(new InstallerMessage
        //{
        //  MessageType = messageType,
        //  MessageRecord = messageRecord
        //});

        using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
        {
          sw.WriteLine($"{DateTime.Now}: ProcessMessage invoked, messageType={messageType}, messageRecord={messageRecord}, buttons={buttons}, icon={icon}, defaultButton={defaultButton}");
        }
      }
      catch(Exception ex)
      {
        using (StreamWriter sw = new StreamWriter(Path.Combine(Path.GetTempPath(), $"{nameof(WpfInstallationWizardApplication)}_installLog.txt"), true))
        {
          sw.WriteLine($"{DateTime.Now}:ProcessMessage Exception occurred. {ex}");
        }
      }

      return MessageResult.OK;
    }

    private void ConfigureServices(IServiceCollection services)
    {
      //register all dialog page view models
      Type viewModelPageBaseType = typeof(WizardPageViewModelBase);
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      foreach (Type viewModelType in executingAssembly.GetTypes().Where(t => t != viewModelPageBaseType
        && viewModelPageBaseType.IsAssignableFrom(t)))
      {
        services.AddSingleton(viewModelType);
      }

      services.AddSingleton<IWizardViewModel>(new WizardViewModel(_session,
        _resourcePath,
        _installerStartedEvent,
        _installerExitedEvent));

      //register all pages
      Type wizardPageBaseType = typeof(WizardPageBase);
      foreach (Type wizardPageType in executingAssembly.GetTypes().Where(t => t != wizardPageBaseType
        && wizardPageBaseType.IsAssignableFrom(t)))
      {
        services.AddTransient(wizardPageType);
      }
      services.AddSingleton<WizardView>();

      services.AddSingleton<WizardPageDataContextToWizardPageConverter>(sp =>
      {
        return new WizardPageDataContextToWizardPageConverter(sp);
      });
    }
  }
}
