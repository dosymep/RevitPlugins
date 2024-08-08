using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit.ServerClient;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitServerFolders.Models;
using RevitServerFolders.Services;
using RevitServerFolders.ViewModels;
using RevitServerFolders.Views;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    internal sealed class FileServerExportCommand : BasePluginCommand {
        public FileServerExportCommand() {
            PluginName = "Экспорт RVT файлов из RS";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.UseXtraProgressDialog<RsViewModel>();
                kernel.UseXtraProgressDialog<FileSystemViewModel>();

                kernel.Bind<RsModelObjectConfig>()
                    .ToMethod(c => RsModelObjectConfig.GetPluginConfig());

                kernel.Bind<IModelObjectService>()
                    .To<RsModelObjectService>();
                kernel.Bind<IModelsExportService>()
                    .To<RvtExportService>()
                    .InSingletonScope();

                kernel.UseXtraOpenFolderDialog<MainWindow>(
                    initialDirectory: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                kernel.Bind<IReadOnlyCollection<IServerClient>>()
                    .ToMethod(c => c.Kernel.Get<Application>()
                        .GetRevitServerNetworkHosts()
                        .Select(item => new ServerClientBuilder()
                            .SetServerName(item)
                            .SetServerVersion(ModuleEnvironment.RevitVersion)
                            .Build())
                        .ToArray());

                kernel.Bind<ViewModels.Rs.MainViewModel>().ToSelf();

                kernel.Bind<RsViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<RsViewModel>());

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
