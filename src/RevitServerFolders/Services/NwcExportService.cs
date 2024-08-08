using System;
using System.IO;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

using dosymep.Revit;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services {
    /// <summary>
    /// Экспортирует rvt файлы в nwc в заданную директорию
    /// </summary>
    internal class NwcExportService : IModelsExportService {
        private const string _nwcSearchPattern = "*.nwc";
        private const string _navisworksViewName = "Navisworks";
        private const string _transactionName = "Смена площадки";
        private readonly RevitRepository _revitRepository;

        public NwcExportService(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }


        public void ExportModelObjects(
            string targetFolder,
            string[] modelFiles,
            IProgress<int> progress = null,
            CancellationToken ct = default) {
            if(string.IsNullOrWhiteSpace(targetFolder)) {
                throw new ArgumentException(nameof(targetFolder));
            }
            if(modelFiles is null) {
                throw new ArgumentNullException(nameof(modelFiles));
            }

            Directory.CreateDirectory(targetFolder);

            var navisFiles = Directory.GetFiles(targetFolder, _nwcSearchPattern);
            foreach(var navisFile in navisFiles) {
                File.SetAttributes(navisFile, FileAttributes.Normal);
                File.Delete(navisFile);
            }

            for(int i = 0; i < modelFiles.Length; i++) {
                progress?.Report(i);
                ct.ThrowIfCancellationRequested();

                var config = FileModelObjectConfig.GetPluginConfig();
                try {
                    ExportDocument(modelFiles[i], targetFolder, config.IsExportRooms);
                } catch(Exception) {
                    // pass
                }
            }
        }


        private void ExportDocument(string fileName, string targetFolder, bool isExportRooms) {
            _revitRepository.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
            _revitRepository.UIApplication.DialogBoxShowing += UIApplicationOnDialogBoxShowing;

            DocumentExtensions.UnloadAllLinks(fileName);
            using(Document document = _revitRepository.OpenDocumentFile(fileName)) {
                try {
                    View3D navisView = new FilteredElementCollector(document)
                        .OfClass(typeof(View3D))
                        .OfType<View3D>()
                        .Where(item => !item.IsTemplate)
                        .FirstOrDefault(item =>
                            item.Name.Equals(_navisworksViewName, StringComparison.OrdinalIgnoreCase));

                    if(navisView == null) {
                        return;
                    }

                    var hasElements = new FilteredElementCollector(document, navisView.Id)
                            .WhereElementIsNotElementType()
#if REVIT_2021_OR_LESS
                        .Where(item =>
                            item.get_Geometry(new Options() {View = navisView})?.Any() == true)
#else
                            .WherePasses(new VisibleInViewFilter(document, navisView.Id))
#endif
                            .Any();

                    if(!hasElements) {
                        return;
                    }

                    ProjectLocation[] projectLocations = document.ProjectLocations
                        .OfType<ProjectLocation>()
                        .ToArray();

                    if(projectLocations.Length == 1) {
                        ExportDocument(fileName, navisView, document, targetFolder, isExportRooms);
                    } else if(projectLocations.Length > 1) {
                        foreach(ProjectLocation projectLocation in projectLocations) {
                            using(Transaction transaction = document.StartTransaction(_transactionName)) {
                                document.ActiveProjectLocation = projectLocation;
                                transaction.Commit();
                            }

                            ExportDocument(fileName, navisView, document, targetFolder, isExportRooms, projectLocation);
                        }
                    }
                } finally {
                    document.Close(false);
                    _revitRepository.Application.FailuresProcessing -= ApplicationOnFailuresProcessing;
                    _revitRepository.UIApplication.DialogBoxShowing -= UIApplicationOnDialogBoxShowing;
                }
            }
        }

        private void ExportDocument(string fileName,
            View3D navisView,
            Document document,
            string targetFolder,
            bool isExportRooms,
            ProjectLocation location = null) {
            string exportFileName = isExportRooms
                ? _revitRepository.GetRoomsFileName(fileName)
                : _revitRepository.GetFileName(fileName);

            NavisworksExportOptions exportOptions = isExportRooms
                ? _revitRepository.GetRoomsExportOptions(navisView)
                : _revitRepository.GetExportOptions(navisView);

            exportFileName += location == null
                ? null
                : "_" + location.Name;

            document.Export(targetFolder, exportFileName, exportOptions);
        }

        private void UIApplicationOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs e) {
            e.OverrideResult(1);
        }

        private void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e) {
            FailuresAccessor accessor = e.GetFailuresAccessor();
            accessor.DeleteAllWarnings();
            accessor.ResolveFailures(accessor.GetFailureMessages());

            ElementId[] elementIds = accessor.GetFailureMessages()
                .SelectMany(item => item.GetFailingElementIds())
                .ToArray();

            if(elementIds.Length > 0) {
                accessor.DeleteElements(elementIds);
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit);
            } else {
                e.SetProcessingResult(FailureProcessingResult.Continue);
            }
        }
    }
}
