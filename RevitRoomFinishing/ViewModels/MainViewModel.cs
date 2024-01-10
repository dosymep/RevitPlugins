using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomFinishing.Models;

namespace RevitRoomFinishing.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private List<Phase> _phases;
        private Phase _selectedPhase;

        private ObservableCollection<ElementsGroupViewModel> _rooms;
        private ObservableCollection<ElementsGroupViewModel> _wallTypes;
        private ObservableCollection<ElementsGroupViewModel> _floorTypes;
        private ObservableCollection<ElementsGroupViewModel> _ceilingTypes;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _phases = _revitRepository.GetPhases();
            _wallTypes = _revitRepository.GetFinishingTypes(BuiltInCategory.OST_Walls);
            _floorTypes = _revitRepository.GetFinishingTypes(BuiltInCategory.OST_Floors);
            _ceilingTypes = _revitRepository.GetFinishingTypes(BuiltInCategory.OST_Ceilings);

            SelectedPhase = _phases[_phases.Count - 1];

            CalculateFinishingCommand = RelayCommand.Create(CalculateFinishing, CanCalculateFinishing);
        }

        public List<Phase> Phases => _phases;

        public ICommand CalculateFinishingCommand { get; }

        public Phase SelectedPhase {
            get => _selectedPhase;
            set {
                this.RaiseAndSetIfChanged(ref _selectedPhase, value);
                OnPropertyChanged("Rooms");
            }
        }

        public ObservableCollection<ElementsGroupViewModel> Rooms {
            get => _revitRepository.GetRoomsOnPhase(_selectedPhase);
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

        }

        private bool CanCalculateFinishing() {
            if(Rooms.Count == 0) {
                ErrorText = "Помещения отсутствуют на выбранной стадии";
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
