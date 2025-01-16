using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
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
  public class WpfEmbeddedUIApplication : Application
  {
    private IServiceProvider _serviceProvider;
    private readonly ISessionProxy _sessionProxy;
    private readonly string _resourcePath;
    private readonly ManualResetEvent _installationStarted;
    private readonly ManualResetEvent _installationExited;

    public WpfEmbeddedUIApplication(ISessionProxy sessionProxy,
      string resourcePath,
      ManualResetEvent installationStarted,
      ManualResetEvent installationExited)
    {
      _sessionProxy = sessionProxy;
      _resourcePath = resourcePath;
      _installationStarted = installationStarted;
      _installationExited = installationExited;

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
        Source = new Uri(@"pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Blue.xaml")
      });

      _serviceProvider.GetRequiredService<IWizardViewModel>().SetPages(
        _serviceProvider.GetRequiredService<InstallCancelledPageViewModel>(),
        _serviceProvider.GetRequiredService<InstallFinishedPageViewModel>(),
        _serviceProvider.GetRequiredService<WelcomePageViewModel>(),
        _serviceProvider.GetRequiredService<LicenseAgreementPageViewModel>(),
        _serviceProvider.GetRequiredService<VerifyReadyPageViewModel>());
    }
    public MessageResult ProcessMessage(InstallMessage messageType, Record messageRecord, MessageButtons buttons, MessageIcon icon, MessageDefaultButton defaultButton)
    {
      if (Application.Current != null)
      {
        //TODO should be using .Reply in the receiver
        //https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/messenger
        StrongReferenceMessenger.Default.Send(new InstallerMessage
        {
          MessageType = messageType,
          MessageRecord = messageRecord
        });
      }
      return MessageResult.OK;
    }

    public void Start()
    {
      Run(_serviceProvider.GetRequiredService<WizardView>());
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

      services.AddSingleton<IWizardViewModel>(new WizardViewModel(_sessionProxy,
        _resourcePath,
        _installationStarted,
        _installationExited,
        DialogCoordinator.Instance,
        StrongReferenceMessenger.Default));

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

      services.AddSingleton<IMessenger>(StrongReferenceMessenger.Default);
      services.AddSingleton<IDialogCoordinator, DialogCoordinator>();
    }
  }
}