using dosymep.WPF.ViewModels;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal class SettingsViewModel : BaseViewModel {
        public SettingsViewModel(int id, int parentId, string settingsName) {
            Id = id;
            ParentId = parentId;
            SettingsName = settingsName;
        }

        public int Id { get; }
        public int ParentId { get; }
        public string SettingsName { get; }

        public virtual void SaveSettings() { }
    }
}