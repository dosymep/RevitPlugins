using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels {
    internal class ErrorsListViewModel : BaseViewModel {
        public string Message { get; set; }
        public string Description { get; set; }

        public ObservableCollection<ErrorElement> Elements { get; set; }
    }

    internal class ErrorElement {
        private readonly Element _element;
        private readonly string _phaseName;
        private readonly string _levelName;

        public ErrorElement(Element element, string phaseName) {
            _element = element;

            _phaseName = phaseName;
            _levelName = _element
                .Document
                .GetElement(_element.LevelId)
                .Name;
        }

        public ElementId ElementId => _element.Id;
        public string ElementName => _element.Name;
        public string CategoryName => _element.Category.Name;
        public string PhaseName => _phaseName;
        public string LevelName => _levelName;
    }
}
