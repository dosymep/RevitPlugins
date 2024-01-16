using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Security;
using System.Xml.Linq;

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

        public ICollection<ElementId> GetFinishingElements(BuiltInCategory category, params string[] typeNames) {
            ElementId parameterId = new ElementId(BuiltInParameter.ELEM_TYPE_PARAM);
            ParameterValueProvider valueProvider = new ParameterValueProvider(parameterId);
            FilterStringContains ruleEvaluator = new FilterStringContains();

            IList<ElementFilter> filters = new List<ElementFilter>();
            foreach(string name in typeNames) {
            #if REVIT_2021_OR_LESS
                FilterStringRule rule = new FilterStringRule(valueProvider, ruleEvaluator, name, false);
            #else
                FilterStringRule rule = new FilterStringRule(valueProvider, ruleEvaluator, name);
            #endif
                ElementParameterFilter parameterFilter = new ElementParameterFilter(rule);
                filters.Add(parameterFilter);
            }

            LogicalOrFilter orFilter = new LogicalOrFilter(filters);

            return new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .WherePasses(orFilter)
                .ToElementIds();
            }

        public ObservableCollection<ElementsGroupViewModel> GetElementTypesOnPhase(ICollection<ElementId> elements, Phase phase) {
            ICollection<ElementOnPhaseStatus> phaseStatuses = new Collection<ElementOnPhaseStatus>() {
                ElementOnPhaseStatus.Existing,
                ElementOnPhaseStatus.Demolished,
                ElementOnPhaseStatus.New,
                ElementOnPhaseStatus.Temporary
            };            

            ElementPhaseStatusFilter phaseFilter = new ElementPhaseStatusFilter(phase.Id, phaseStatuses);

            var finishingTypes = new FilteredElementCollector(Document, elements)
                .WherePasses(phaseFilter)
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
