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

        private ObservableCollection<string> _rooms;

        private string _errorText;
        private string _saveProperty;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            _phases = _revitRepository.GetPhases();            

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public List<Phase> Phases => _phases;

        public Phase SelectedPhase {
            get => _selectedPhase;
            set {
                this.RaiseAndSetIfChanged(ref _selectedPhase, value);
                OnPropertyChanged("Rooms");
            }
        }

        public ObservableCollection<string> Rooms {
            get => _revitRepository.GetRoomNamesOnPhase(_selectedPhase);
            set => this.RaiseAndSetIfChanged(ref _rooms, value);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
        }
        
        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(SaveProperty)) {
                ErrorText = "Введите значение сохраняемого свойства.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
