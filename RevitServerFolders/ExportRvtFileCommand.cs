#region Namespaces
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Interop;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;
using dosymep.SimpleServices;

using RevitServerFolders.Export;

#endregion

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    public class ExportRvtFileCommand : BasePluginCommand {
        public ExportRvtFileCommand() {
            PluginName = "Экспорт RVT из RS";
        }
        protected override void Execute(UIApplication uiApplication) {
            var application = uiApplication.Application;

            var exportRvtFileViewModel = new ExportRvtFileViewModel(application.VersionNumber, application.GetRevitServerNetworkHosts(), ExportRvtFileConfig.GetExportRvtFileConfig());

            var exportWindow = new ExportRvtFileWindow { DataContext = exportRvtFileViewModel };

            exportRvtFileViewModel.Owner = exportWindow;
            if(exportWindow.ShowDialog() == true) {
                exportRvtFileViewModel = (ExportRvtFileViewModel) exportWindow.DataContext;

                SaveExportRvtFileConfig(exportRvtFileViewModel);
                DetachRevitFiles(exportRvtFileViewModel);
                UnloadAllLinks(exportRvtFileViewModel);
                ExportFilesToNavisworks(application, exportRvtFileViewModel);

                GetPlatformService<INotificationService>()
                    .CreateNotification(PluginName, "Выполнение скрипта завершено успешно.", "C#")
                    .ShowAsync();
            } else {
                GetPlatformService<INotificationService>()
                    .CreateWarningNotification(PluginName, "Выполнение скрипта отменено.")
                    .ShowAsync();
            }
        }

        private static void ExportFilesToNavisworks(Application application, ExportRvtFileViewModel exportRvtFileViewModel) {
            if(exportRvtFileViewModel.WithNwcFiles) {
                new ExportFilesToNavisworksCommand() {
                    Application = application,
                    SourceFolderName = exportRvtFileViewModel.TargetRvtFolder,
                    TargetFolderName = exportRvtFileViewModel.TargetNwcFolder,

                    WithRooms = exportRvtFileViewModel.WithRooms,
                    WithLinkedFiles = exportRvtFileViewModel.WithLinkedFiles,
                    CleanTargetFolder = exportRvtFileViewModel.CleanTargetNwcFolder
                }.Execute();
            }
        }

        private static void UnloadAllLinks(ExportRvtFileViewModel exportRvtFileViewModel) {
            new UnloadAllLinksCommand() {
                SourceFolderName = exportRvtFileViewModel.TargetRvtFolder
            }.Execute();
        }

        private static void DetachRevitFiles(ExportRvtFileViewModel exportRvtFileViewModel) {
            new DetachRevitFilesCommand() {
                ServerName = exportRvtFileViewModel.ServerName,
                RevitVersion = exportRvtFileViewModel.RevitVersion,
                WithSubFolders = exportRvtFileViewModel.WithSubFolders,
                FolderName = exportRvtFileViewModel.SourceRvtFolder,
                TargetFolderName = exportRvtFileViewModel.TargetRvtFolder,
                CleanTargetFolder = exportRvtFileViewModel.CleanTargetRvtFolder
            }.Execute();
        }

        private static void SaveExportRvtFileConfig(ExportRvtFileViewModel exportRvtFileViewModel) {
            ExportRvtFileConfig exportConfig = ExportRvtFileConfig.GetExportRvtFileConfig();

            exportConfig.ServerName = exportRvtFileViewModel.ServerName;
            exportConfig.WithRooms = exportRvtFileViewModel.WithRooms;
            exportConfig.WithNwcFiles = exportRvtFileViewModel.WithNwcFiles;
            exportConfig.SourceRvtFolder = exportRvtFileViewModel.SourceRvtFolder;
            exportConfig.TargetNwcFolder = exportRvtFileViewModel.TargetNwcFolder;
            exportConfig.TargetRvtFolder = exportRvtFileViewModel.TargetRvtFolder;
            exportConfig.WithSubFolders = exportRvtFileViewModel.WithSubFolders;
            exportConfig.CleanTargetNwcFolder = exportRvtFileViewModel.CleanTargetNwcFolder;
            exportConfig.CleanTargetRvtFolder = exportRvtFileViewModel.CleanTargetRvtFolder;

            exportConfig.SaveProjectConfig();
        }
    }
}
