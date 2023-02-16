﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopingZones.Models;

namespace RevitCopingZones.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly CopingZonesConfig _copingZonesConfig;

        private string _errorText;
        private Area[] _selectedAreas;
        private FloorPlanViewModel _floorPlan;
        private ObservableCollection<FloorPlanViewModel> _floorPlans;

        public MainViewModel(RevitRepository revitRepository, CopingZonesConfig copingZonesConfig) {
            _revitRepository = revitRepository;
            _copingZonesConfig = copingZonesConfig;

            LoadViewCommand = new RelayCommand(LoadView);
            SelectAreasCommand = new RelayCommand(SelectAreas);
            ExecuteViewCommand = new RelayCommand(ExecuteView, CanExecuteView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand SelectAreasCommand { get; }
        public ICommand ExecuteViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public FloorPlanViewModel FloorPlan {
            get => _floorPlan;
            set => this.RaiseAndSetIfChanged(ref _floorPlan, value);
        }

        public ObservableCollection<FloorPlanViewModel> FloorPlans {
            get => _floorPlans;
            set => this.RaiseAndSetIfChanged(ref _floorPlans, value);
        }

        private void LoadView(object p) {
            var floorPlans = _revitRepository.GetFloorPlans()
                .Select(item => new FloorPlanViewModel(item));
            FloorPlans = new ObservableCollection<FloorPlanViewModel>(floorPlans);
        }

        private void SelectAreas(object p) {
            _selectedAreas = _revitRepository.GetSelectedAreas().ToArray();
        }

        private void ExecuteView(object p) {
            
        }

        private bool CanExecuteView(object p) {
            if(_selectedAreas == null 
               || _selectedAreas.Length == 0) {
                ErrorText = "Выберите зоны.";
                return false;
            }

            if(!FloorPlans.Any(item => item.CanCopyAreas)) {
                ErrorText = "Выберите этажи.";
                return false;
            }
            
            ErrorText = null;
            return true;
        }
    }
}