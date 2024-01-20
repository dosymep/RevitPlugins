using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRoomFinishing.ViewModels {
    internal class ErrorsViewModel : BaseViewModel {
        public string Message { get; set; }
        public string Description { get; set; }

        public ObservableCollection<ErrorElementInfo> Elements { get; set; }
    }

    internal class ErrorElementInfo {
        private Element _element;
        public ErrorElementInfo(Element element) {
            _element = element;
        }

        public string ElementName => _element.Name;
        public ElementId ElementId  => _element.Id;
        public string CategoryName  => _element.Category.Name;
    }
}
