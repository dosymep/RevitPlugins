using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;


using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitMechanicalSpecification.Models;
using RevitMechanicalSpecification.ViewModels;
using RevitMechanicalSpecification.Views;

namespace RevitMechanicalSpecification {
    [Transaction(TransactionMode.Manual)]
    public class RevitMechanicalSpecificationCommand : BasePluginCommand {
        public RevitMechanicalSpecificationCommand() {
            PluginName = "RevitMechanicalSpecification";
        }

        protected override void Execute(UIApplication uiApplication) {
            MessageBox.Show("Ура");
			//using(IKernel kernel = uiApplication.CreatePlatformServices()) {
   //             kernel.Bind<RevitRepository>()
   //                 .ToSelf()
   //                 .InSingletonScope();
					
			//	kernel.Bind<PluginConfig>()
   //                 .ToMethod(c => PluginConfig.GetPluginConfig());
				
			//	kernel.Bind<MainViewModel>().ToSelf();
			//	kernel.Bind<MainWindow>().ToSelf()
   //                 .WithPropertyValue(nameof(Window.DataContext), 
   //                     c => c.Kernel.Get<MainViewModel>())
   //                 .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
   //                     c => c.Kernel.Get<ILocalizationService>());
				
   //             string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

   //             //kernel.UseXtraLocalization(
   //             //    $"/{assemblyName};component/Localization/Language.xaml",
   //             //    CultureInfo.GetCultureInfo("ru-RU"));
                
			//	Notification(kernel.Get<MainWindow>());
			//}
        }
    }
}
