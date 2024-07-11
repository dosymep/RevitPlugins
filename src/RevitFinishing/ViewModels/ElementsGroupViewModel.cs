using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels
{
    internal class ElementsGroupViewModel : BaseViewModel {
        private readonly string _name;
        private readonly IReadOnlyCollection<Element> _elements;

        private bool _isChecked;

        public ElementsGroupViewModel(string name, IEnumerable<Element> elements) {
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
