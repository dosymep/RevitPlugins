using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitPlatformSettings.Factories;
using RevitPlatformSettings.Model;
using RevitPlatformSettings.Services;
using RevitPlatformSettings.ViewModels;
using RevitPlatformSettings.ViewModels.Settings;
using RevitPlatformSettings.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitPlatformSettings {
    [Transaction(TransactionMode.Manual)]
    public class PlatformSettingsCommand : BasePluginCommand {
        public PlatformSettingsCommand() {
            PluginName = "Настройки платформы";
        }

        protected override void Execute(UIApplication uiApplication) {
            Notification(OpenSettingsWindow(uiApplication));
        }

        public bool? OpenSettingsWindow(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<UIApplication>()
                    .ToConstant(uiApplication)
                    .InTransientScope();
                kernel.Bind<Application>()
                    .ToConstant(uiApplication.Application)
                    .InTransientScope();


                kernel.Bind<BuiltinExtension>().ToSelf();
                kernel.Bind<ThirdPartyExtension>().ToSelf();

                kernel.Bind<IExtensionFactory<BuiltinExtension>>()
                    .To<ExtensionFactory<BuiltinExtension>>();

                kernel.Bind<IExtensionFactory<ThirdPartyExtension>>()
                    .To<ExtensionFactory<ThirdPartyExtension>>();

                kernel.Bind<IExtensionsService<BuiltinExtension>>()
                    .To<BuiltinExtensionsService>();

                kernel.Bind<IExtensionsService<ThirdPartyExtension>>()
                    .To<ThirdPartyExtensionsService>();

                kernel.Bind<IPyRevitExtensionsService>()
                    .To<PyRevitExtensionsService>();

                kernel.Bind<IExtensionViewModelFactory>()
                    .To<ExtensionViewModelFactory>();

                kernel.Bind<ISettingsViewModelFactory>()
                    .To<SettingsViewModelFactory>();

                kernel.Bind<SettingsViewModel>().ToSelf();
                kernel.Bind<GeneralSettingsViewModel>().ToSelf();
                kernel.Bind<ExtensionsSettingsViewModel>().ToSelf();
                kernel.Bind<RevitParamsSettingsViewModel>().ToSelf();
                kernel.Bind<AboutSettingsViewModel>().ToSelf();

                kernel.Bind<MainViewModel>().ToSelf()
                    .InSingletonScope();

                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(ThemedPlatformWindow.LocalizationService),
                        c => c.Kernel.Get<ILocalizationService>());
                
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                
                return kernel.Get<MainWindow>().ShowDialog();
            }
        }
    }
}
