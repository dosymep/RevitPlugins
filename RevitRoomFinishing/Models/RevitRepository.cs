using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitRoomFinishing.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public ObservableCollection<string> GetRoomNamesOnPhase(Phase phase) {
            var rooms = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .OfType<Room>()
                .Where(x => x.GetParamValueOrDefault<ElementId>(BuiltInParameter.ROOM_PHASE) == phase.Id)
                .Select(x => x.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, "<Без имени>"))
                .Distinct()
                .ToList();

            return new ObservableCollection<string>(rooms);
        }

        public List<Phase> GetPhases() {
            return new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Phases)
                .OfType<Phase>()
                .ToList();
        }
    }
}
