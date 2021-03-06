using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

using dosymep.WPF.Views.Revit;
using dosymep.WPF.Commands;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace RevitServerFolders.Export {
    public class ExportRvtFileViewModel : INotifyPropertyChanged, IDataErrorInfo {
        private string _serverName;
        private IList<string> _serverNames;

        private bool _withRooms;
        private bool _withSubFolders;
        private string _sourceRvtFolder;
        private string _targetRvtFolder;

        private bool _withNwcFiles;
        private string _targetNwcFolder;

        private bool _cleanTargetRvtFolder;
        private bool _cleanTargetNwcFolder;
        private string _error;
        private bool _withLinkedFiles;

        private RevitServerViewModel _revitServerViewModel;

        public ExportRvtFileViewModel() { }

        public ExportRvtFileViewModel(string revitVersion, IList<string> serverNames,
            ExportRvtFileConfig exportRvtFileConfig) {
            RevitVersion = revitVersion;
            ServerNames = serverNames;

            ServerName = exportRvtFileConfig.ServerName ?? ServerNames?.FirstOrDefault();
            WithSubFolders = exportRvtFileConfig.WithSubFolders;
            SourceRvtFolder = exportRvtFileConfig.SourceRvtFolder;
            TargetRvtFolder = exportRvtFileConfig.TargetRvtFolder;
            WithRooms = exportRvtFileConfig.WithRooms;
            WithNwcFiles = exportRvtFileConfig.WithNwcFiles;
            TargetNwcFolder = exportRvtFileConfig.TargetNwcFolder;
            CleanTargetRvtFolder = exportRvtFileConfig.CleanTargetRvtFolder;
            CleanTargetNwcFolder = exportRvtFileConfig.CleanTargetNwcFolder;

            SelectSourceRvtFolderCommand = new RelayCommand(p => {
                RevitServerExplorerWindow selectWindow = GetSelectWindow();
                if(selectWindow.ShowDialog() == true) {
                    SourceRvtFolder = selectWindow.ViewModel.SelectedItem.Path;
                }
            });

            SelectTargetRvtFolderCommand = new RelayCommand(SelectTargetRvtFolder);
            SelectTargetNwcFolderCommand = new RelayCommand(SelectTargetNwcFolder);
        }

        private RevitServerExplorerWindow GetSelectWindow() {
            if(_revitServerViewModel == null || !ServerName.Equals(_revitServerViewModel.ServerName)) {
                _revitServerViewModel = new RevitServerViewModel(ServerName, RevitVersion);
            }

            return new RevitServerExplorerWindow() {ViewModel = _revitServerViewModel, Owner = Owner};
        }

        public string RevitVersion { get; }
        public System.Windows.Window Owner { get; set; }

        public string ServerName {
            get => _serverName;
            set {
                _serverName = value;
                OnPropertyChanged(nameof(ServerName));

                SourceRvtFolder = null;
            }
        }

        public IList<string> ServerNames {
            get => _serverNames;
            set {
                _serverNames = value;
                OnPropertyChanged(nameof(ServerNames));
            }
        }

        public bool WithRooms {
            get => _withRooms;
            set {
                _withRooms = value;
                OnPropertyChanged(nameof(WithRooms));
            }
        }

        public bool WithSubFolders {
            get => _withSubFolders;
            set {
                _withSubFolders = value;
                OnPropertyChanged(nameof(WithSubFolders));
            }
        }

        public string SourceRvtFolder {
            get => _sourceRvtFolder;
            set {
                _sourceRvtFolder = value;
                OnPropertyChanged(nameof(SourceRvtFolder));
            }
        }

        public string TargetRvtFolder {
            get => _targetRvtFolder;
            set {
                _targetRvtFolder = value;
                OnPropertyChanged(nameof(TargetRvtFolder));
            }
        }

        public bool CleanTargetRvtFolder {
            get => _cleanTargetRvtFolder;
            set {
                _cleanTargetRvtFolder = value;
                OnPropertyChanged(nameof(CleanTargetRvtFolder));
            }
        }

        public bool WithNwcFiles {
            get => _withNwcFiles;
            set {
                _withNwcFiles = value;
                OnPropertyChanged(nameof(WithNwcFiles));
                OnPropertyChanged(nameof(TargetNwcFolder));
            }
        }

        public string TargetNwcFolder {
            get => _targetNwcFolder;
            set {
                _targetNwcFolder = value;
                OnPropertyChanged(nameof(TargetNwcFolder));
            }
        }

        public bool CleanTargetNwcFolder {
            get => _cleanTargetNwcFolder;
            set {
                _cleanTargetNwcFolder = value;
                OnPropertyChanged(nameof(CleanTargetNwcFolder));
            }
        }
        
        public bool WithLinkedFiles {
            get => _withLinkedFiles;
            set {
                _withLinkedFiles = value;
                OnPropertyChanged(nameof(WithLinkedFiles));
            }
        }

        public ICommand SelectSourceRvtFolderCommand { get; set; }
        public ICommand SelectTargetRvtFolderCommand { get; set; }
        public ICommand SelectTargetNwcFolderCommand { get; set; }

        private void SelectTargetRvtFolder(object p) {
            using(var dialog = new CommonOpenFileDialog()) {
                dialog.IsFolderPicker = true;
                dialog.Title = "Папка сохранения открепленных RVT-файлов";
                dialog.InitialDirectory =
                    TargetRvtFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                    TargetRvtFolder = dialog.FileName;
                }
            }
        }

        private void SelectTargetNwcFolder(object p) {
            using(var dialog = new CommonOpenFileDialog()) {
                dialog.IsFolderPicker = true;
                dialog.Title = "Папка сохранения NWC-файлов";
                dialog.InitialDirectory =
                    TargetNwcFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                    TargetNwcFolder = dialog.FileName;
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDataErrorInfo

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        public bool IsValid {
            get { return string.IsNullOrEmpty(Error); }
        }

        public string Error {
            get => _error;
            private set {
                if(!string.Equals(_error, value)) {
                    _error = value;
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public string this[string columnName] {
            get { return Validate(columnName); }
        }

        private string Validate(string propertyName) {
            string error = null;
            switch(propertyName) {
                case nameof(ServerName):
                error = string.IsNullOrEmpty(ServerName) ? "Выберите имя сервера!" : null;
                break;
                case nameof(SourceRvtFolder):
                error = string.IsNullOrEmpty(SourceRvtFolder) ? "Выберите папку с файлами Revit!" : null;
                break;
                case nameof(TargetRvtFolder):
                error = string.IsNullOrEmpty(TargetRvtFolder) ? "Выберите папку сохранения открепленных файлов Revit!" : null;
                break;
                case nameof(TargetNwcFolder):
                error = string.IsNullOrEmpty(TargetNwcFolder) && WithNwcFiles ? "Выберите папку сохранения NWC-файлов!" : null;
                break;
            }

            _errors[propertyName] = error;
            Error = _errors.Values.FirstOrDefault(item => !string.IsNullOrEmpty(item));

            return error;
        }

        #endregion
    }
}