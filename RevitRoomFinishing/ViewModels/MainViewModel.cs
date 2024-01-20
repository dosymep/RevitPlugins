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
                
        private readonly ICollection<ElementId> _walls;
        private readonly ICollection<ElementId> _floors;
        private readonly ICollection<ElementId> _ceilings;
                
        private ObservableCollection<ElementsGroupViewModel> _rooms;
        private ObservableCollection<ElementsGroupViewModel> _wallTypes;
        private ObservableCollection<ElementsGroupViewModel> _floorTypes;
        private ObservableCollection<ElementsGroupViewModel> _ceilingTypes;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _walls = _revitRepository.GetFinishingElements(BuiltInCategory.OST_Walls, "(О) Стена", "(О) Плинтус");
            _floors = _revitRepository.GetFinishingElements(BuiltInCategory.OST_Floors, "(АР)");
            _ceilings = _revitRepository.GetFinishingElements(BuiltInCategory.OST_Ceilings, "(О) Потолок");            

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
                _wallTypes = _revitRepository.GetFinishingGroupsByPhase(_walls, _selectedPhase);
                _floorTypes = _revitRepository.GetFinishingGroupsByPhase(_floors, _selectedPhase);
                _ceilingTypes = _revitRepository.GetFinishingGroupsByPhase(_ceilings, _selectedPhase);
                OnPropertyChanged("Rooms");
                OnPropertyChanged("WallTypes");
                OnPropertyChanged("FloorTypes");
                OnPropertyChanged("CeilingTypes");
            }
        }

        public ObservableCollection<ElementsGroupViewModel> Rooms {
            get => _rooms;
            set => this.RaiseAndSetIfChanged(ref _rooms, value);
        }

        public ObservableCollection<ElementsGroupViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

        public ObservableCollection<ElementsGroupViewModel> FloorTypes {
            get => _floorTypes;
            set => this.RaiseAndSetIfChanged(ref _floorTypes, value);
        }

        public ObservableCollection<ElementsGroupViewModel> CeilingTypes {
            get => _ceilingTypes;
            set => this.RaiseAndSetIfChanged(ref _ceilingTypes, value);
        }



        private void CalculateFinishing() {
            List<ElementsGroupViewModel> allFinishings = WallTypes
                .Concat(CeilingTypes)
                .Concat(FloorTypes)
                .ToList();
                
            FinishingCalculator calculator = new FinishingCalculator(Rooms, allFinishings);

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
            if(!WallTypes.Any(w => w.IsChecked) && !FloorTypes.Any(f => f.IsChecked) && !CeilingTypes.Any(c => c.IsChecked)) {
                ErrorText = "Отделка не выбрана";
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
