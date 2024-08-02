using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitPlatformSettings.ViewModels.Settings {
    internal class RevitParamsSettingsViewModel : SettingsViewModel {
        public RevitParamsSettingsViewModel(int id, int parentId, string settingsName)
            : base(id, parentId, settingsName) {
            IEnumerable<RevitParam> revitParams = SharedParamsConfig.Instance.GetRevitParams()
                .Union(ProjectParamsConfig.Instance.GetRevitParams());

            RevitParams = new ObservableCollection<RevitParam>(revitParams);
        }

        public ObservableCollection<RevitParam> RevitParams { get; }
    }
}
