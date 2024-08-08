using System.IO;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitServerFolders.Models {
    internal class RevitRepository {
        private const string _roomsSuffix = "_ROOMS.rvt";

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public Document OpenDocumentFile(string fileName) {
            var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(fileName);
            return Application.OpenDocumentFile(
                modelPath,
                new OpenOptions() {
                    AllowOpeningLocalByWrongUser = true,
                    OpenForeignOption = OpenForeignOption.Open,
                    DetachFromCentralOption = DetachFromCentralOption.DetachAndDiscardWorksets,
                });
        }

        public string GetFileName(string fileName) {
            return Path.GetFileNameWithoutExtension(fileName);
        }

        public string GetRoomsFileName(string fileName) {
            return Path.GetFileNameWithoutExtension(fileName) + _roomsSuffix;
        }

        public NavisworksExportOptions GetExportOptions(View exportView) {
            return new NavisworksExportOptions {
                ViewId = exportView.Id,
                ExportScope = NavisworksExportScope.View,
                Parameters = NavisworksParameters.All,
                Coordinates = NavisworksCoordinates.Shared,
                FacetingFactor = 5.0,
                ExportElementIds = true,
                ConvertElementProperties = true,
                ExportUrls = false,
                ConvertLights = false,
                ExportRoomAsAttribute = false,
                ConvertLinkedCADFormats = false,
                ExportLinks = false,
                ExportParts = false,
                FindMissingMaterials = false,
                DivideFileIntoLevels = true,
                ExportRoomGeometry = false
            };
        }

        public NavisworksExportOptions GetRoomsExportOptions(View exportView) {
            NavisworksExportOptions exportOptions = GetExportOptions(exportView);
            exportOptions.ExportRoomGeometry = true;
            exportOptions.ExportRoomAsAttribute = true;

            return exportOptions;
        }
    }
}
