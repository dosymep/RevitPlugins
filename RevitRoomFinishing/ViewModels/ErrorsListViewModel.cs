using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitRoomFinishing.ViewModels {
    internal class ErrorsListViewModel : BaseViewModel {
        public string Message { get; set; }
        public string Description { get; set; }

        public ObservableCollection<ErrorElement> Elements { get; set; }
    }

    internal class ErrorElement {
        private Element _element;
        public ErrorElement(Element element) {
            _element = element;
        }

        public ElementId ElementId  => _element.Id;
        public string ElementName => _element.Name;
        public string CategoryName  => _element.Category.Name;
        public string PhaseName  => _element.GetParamValue<string>(BuiltInParameter.ROOM_PHASE);
        public ElementId LevelName  => _element.LevelId;
    }
}
