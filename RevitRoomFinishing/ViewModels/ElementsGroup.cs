using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitRoomFinishing.ViewModels
{
    class ElementsGroup : BaseViewModel {
        private readonly string _name;
        private readonly IReadOnlyCollection<Element> _elements;
        private bool _isChecked;

        public ElementsGroup(string name, IEnumerable<Element> elements) {
            _elements = elements.ToList();
            _name = name;
        }

        public IReadOnlyCollection<Element> Elements => _elements;
        public string Name => _name;
        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);
        }
    }
}
