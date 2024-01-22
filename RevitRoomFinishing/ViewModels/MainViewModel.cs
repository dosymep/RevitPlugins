using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomFinishing.Models;
using RevitRoomFinishing.Views;

namespace RevitRoomFinishing.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private readonly List<Phase> _phases;
        private Phase _selectedPhase;
                
        private ObservableCollection<ElementsGroupViewModel> _rooms;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _phases = _revitRepository.GetPhases(); 
            SelectedPhase = _phases[_phases.Count - 1];

            CalculateFinishingCommand = RelayCommand.Create(CalculateFinishing, CanCalculateFinishing);
        }
        public ICommand CalculateFinishingCommand { get; }

        public List<Phase> Phases => _phases;

        public Phase SelectedPhase {
            get => _selectedPhase;
            set {
                this.RaiseAndSetIfChanged(ref _selectedPhase, value);
                _rooms = _revitRepository.GetRoomsOnPhase(_selectedPhase);
                OnPropertyChanged("Rooms");
            }
        }

        public ObservableCollection<ElementsGroupViewModel> Rooms {
            get => _rooms;
            set => this.RaiseAndSetIfChanged(ref _rooms, value);
        }

        private void CalculateFinishing() {
            ICollection<Element> walls = _revitRepository.GetFinishingElementsOnPhase(BuiltInCategory.OST_Walls, SelectedPhase, "(О) Стена");
            ICollection<Element> baseboards = _revitRepository.GetFinishingElementsOnPhase(BuiltInCategory.OST_Walls, SelectedPhase, "(О) Плинтус");
            ICollection<Element> floors = _revitRepository.GetFinishingElementsOnPhase(BuiltInCategory.OST_Floors, SelectedPhase, "(АР) ");
            ICollection<Element> ceilings = _revitRepository.GetFinishingElementsOnPhase(BuiltInCategory.OST_Ceilings, SelectedPhase, "(О) Потолок");

            IEnumerable<Element> finishingElements = walls
                .Concat(baseboards)
                .Concat(floors)
                .Concat(ceilings);

            IEnumerable<Element> selectedRooms = Rooms
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Elements);

            FinishingCalculator calculator = new FinishingCalculator(selectedRooms, finishingElements);

            if(calculator.ErrorElements.Any()) {
                var window = new ErrorsInfoWindow() {
                    DataContext = new ErrorsViewModel() {
                        Elements = new ObservableCollection<ErrorElementInfo>(calculator.ErrorElements)
                    }
                };
                window.Show();
                return;
            }

            if(calculator.WarningElements.Any()) {
                var window = new ErrorsInfoWindow() {
                    DataContext = new ErrorsViewModel() {
                        Elements = new ObservableCollection<ErrorElementInfo>(calculator.WarningElements)
                    }
                };
                window.Show();
            }

            List<FinishingElement> finishings = calculator.Finishings;
            using(Transaction t = _revitRepository.Document.StartTransaction("Заполнить параметры отделки")) {
                foreach(var element in finishings) {
                    element.UpdateFinishingParameters();
                }
                t.Commit();
            }
        }

        private bool CanCalculateFinishing() {
            if(Rooms.Count == 0) {
                ErrorText = "Помещения отсутствуют на выбранной стадии";
                return false;
            }
            if(!Rooms.Any(x => x.IsChecked)) {
                ErrorText = "Помещения не выбраны";
                return false;
            }

            ErrorText = null;
            return true;
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}
