using RevitPlatformSettings.ViewModels;
using RevitPlatformSettings.ViewModels.Settings;

namespace RevitPlatformSettings.Factories {
    internal interface ISettingsViewModelFactory {
        T Create<T>(int id, int parentId, string settingsName) where T : SettingsViewModel;
    }
}