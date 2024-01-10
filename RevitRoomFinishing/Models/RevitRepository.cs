using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Security;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using Ninject.Activation;

using RevitRoomFinishing.ViewModels;

namespace RevitRoomFinishing.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public ObservableCollection<ElementsGroupViewModel> GetRoomsOnPhase(Phase phase) {
            ParameterValueProvider valueProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_PHASE));
            FilterNumericEquals ruleEvaluator = new FilterNumericEquals();
            FilterElementIdRule filterRule = new FilterElementIdRule(valueProvider, ruleEvaluator, phase.Id);
            ElementParameterFilter parameterFilter = new ElementParameterFilter(filterRule);

            var rooms = new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WherePasses(parameterFilter)
                .OfType<Room>()
                .GroupBy(x => x.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, "<Без имени>"))
                .Select(x => new ElementsGroupViewModel(x.Key.ToString(), x))
                .ToList();

            return new ObservableCollection<ElementsGroupViewModel>(rooms);
        }

        public bool CheckIsElementOnPhase(Element element, Phase phase) {
            int phaseStatus = (int) element.GetPhaseStatus(phase.Id);
            if(phaseStatus > 1 && phaseStatus < 6) {
                return true;
            }
            return false;
        }

        public ObservableCollection<ElementsGroupViewModel> GetFinishingTypes(BuiltInCategory category) {
            var finishingTypes = new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .Select(x => Document.GetElement(x.GetTypeId()))
                .GroupBy(x => x.Name)
                .Select(x => new ElementsGroupViewModel(x.Key, x))
                .ToList();

            return new ObservableCollection<ElementsGroupViewModel>(finishingTypes);
        }

        public List<Phase> GetPhases() {
            return Document.Phases.OfType<Phase>().ToList();
        }
    }
}
