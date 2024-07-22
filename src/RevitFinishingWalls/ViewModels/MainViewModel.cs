using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Models.Enums;
using RevitFinishingWalls.Services;
using RevitFinishingWalls.Services.Creation;

namespace RevitFinishingWalls.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly IRoomFinisher _roomFinisher;

        /// <summary>Максимальная допустимая отметка верха отделочной стены в мм</summary>
        private const int _wallTopMaxElevationMM = 50000;
        /// <summary>Максимальное смещение низа стены вверх в мм</summary>
        private const int _wallBaseMaxOffsetMM = 5000;
        /// <summary>Минимальное смещение низа стены вниз в мм</summary>
        private const int _wallBaseMinOffsetMM = -5000;
        /// <summary>Максимальное смещение стены внутрь помещения в мм</summary>
        private const int _wallSideMaxOffsetMM = 200;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            IRoomFinisher roomFinisher
            ) {
            _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _roomFinisher = roomFinisher ?? throw new ArgumentNullException(nameof(roomFinisher));

            RoomGetterModes = new ObservableCollection<RoomGetterMode>(
                Enum.GetValues(typeof(RoomGetterMode)).Cast<RoomGetterMode>());
            WallElevationModes = new ObservableCollection<WallElevationMode>(
                Enum.GetValues(typeof(WallElevationMode)).Cast<WallElevationMode>());
            WallTypes = new ObservableCollection<WallTypeViewModel>(
                _revitRepository.GetWallTypes().Select(wt => new WallTypeViewModel(wt)).OrderBy(wt => wt.Name));

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            LoadConfigCommand = RelayCommand.Create(LoadConfig);
        }

        public ICommand LoadConfigCommand { get; }

        public ICommand AcceptViewCommand { get; }


        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }


        public ObservableCollection<RoomGetterMode> RoomGetterModes { get; }

        private RoomGetterMode _selectedRoomGetterMode;
        public RoomGetterMode SelectedRoomGetterMode {
            get => _selectedRoomGetterMode;
            set => RaiseAndSetIfChanged(ref _selectedRoomGetterMode, value);
        }


        public ObservableCollection<WallTypeViewModel> WallTypes { get; }

        private WallTypeViewModel _selectedWallType;
        public WallTypeViewModel SelectedWallType {
            get => _selectedWallType;
            set => RaiseAndSetIfChanged(ref _selectedWallType, value);
        }


        private string _wallHeightByUser;
        public string WallElevationByUser {
            get => _wallHeightByUser;
            set => RaiseAndSetIfChanged(ref _wallHeightByUser, value);
        }


        public bool IsWallHeightByUserEnabled => SelectedWallElevationMode == WallElevationMode.ManualHeight;


        public ObservableCollection<WallElevationMode> WallElevationModes { get; }

        private WallElevationMode _selectedWallHeightMode;
        public WallElevationMode SelectedWallElevationMode {
            get => _selectedWallHeightMode;
            set {
                RaiseAndSetIfChanged(ref _selectedWallHeightMode, value);
                OnPropertyChanged(nameof(IsWallHeightByUserEnabled));
            }
        }


        private string _wallBaseOffset;
        public string WallBaseOffset {
            get => _wallBaseOffset;
            set => RaiseAndSetIfChanged(ref _wallBaseOffset, value);
        }

        private string _wallSideOffset;
        public string WallSideOffset {
            get => _wallSideOffset;
            set => RaiseAndSetIfChanged(ref _wallSideOffset, value);
        }

        private void AcceptView() {
            SaveConfig();
            var errors = _roomFinisher.CreateWallsFinishing(_pluginConfig);
            if(errors.Count > 0) {
                var errorMsgService = ServicesProvider.GetPlatformService<RichErrorMessageService>();
                errorMsgService.ShowErrorWindow(errors);
            }
        }

        private bool CanAcceptView() {
            if(SelectedWallType is null) {
                ErrorText = "Задайте тип отделочных стен";
                return false;
            }
            if(double.TryParse(WallBaseOffset, out double baseOffset)) {
                if(baseOffset < _wallBaseMinOffsetMM) {
                    ErrorText = "Слишком большое смещение вниз";
                    return false;
                } else if(baseOffset > _wallBaseMaxOffsetMM) {
                    ErrorText = "Слишком большое смещение вверх";
                    return false;
                }
            } else {
                ErrorText = "Смещение должно быть числом";
                return false;
            }
            if(SelectedWallElevationMode == WallElevationMode.ManualHeight) {
                if(double.TryParse(WallElevationByUser, out double height)) {
                    if(height <= 0) {
                        ErrorText = "Отметка верха должна быть больше 0";
                        return false;
                    } else if(height > _wallTopMaxElevationMM) {
                        ErrorText = "Слишком большая отметка верха стены";
                        return false;
                    } else if(height <= baseOffset) {
                        ErrorText = "Отметка верха должна быть больше смещения";
                        return false;
                    }
                } else {
                    ErrorText = "Отметка верха должна быть числом";
                    return false;
                }
            }
            if(double.TryParse(WallSideOffset, out double sideOffset)) {
                if(sideOffset < 0) {
                    ErrorText = "Смещение внутрь не должно быть меньше 0";
                    return false;
                } else if(sideOffset > _wallSideMaxOffsetMM) {
                    ErrorText = "Слишком большое смещение внутрь";
                    return false;
                }
            } else {
                ErrorText = "Смещение внутрь должно быть числом";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            SelectedRoomGetterMode = _pluginConfig.RoomGetterMode;
            SelectedWallElevationMode = _pluginConfig.WallElevationMode;
            WallElevationByUser = _pluginConfig.WallElevationMm.ToString();
            WallBaseOffset = _pluginConfig.WallBaseOffsetMm.ToString();
            WallSideOffset = _pluginConfig.WallSideOffsetMm.ToString();
            SelectedWallType = WallTypes.FirstOrDefault(wtvm => wtvm.WallTypeId == _pluginConfig.WallTypeId);

            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            _pluginConfig.RoomGetterMode = SelectedRoomGetterMode;
            _pluginConfig.WallElevationMode = SelectedWallElevationMode;
            _pluginConfig.WallBaseOffsetMm = double.TryParse(WallBaseOffset, out double baseOffset) ? baseOffset : 0;
            _pluginConfig.WallSideOffsetMm = double.TryParse(WallSideOffset, out double sideOffset) ? sideOffset : 0;
            _pluginConfig.WallElevationMm = double.TryParse(WallElevationByUser, out double height) ? height : 0;
            _pluginConfig.WallTypeId = SelectedWallType.WallTypeId;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
