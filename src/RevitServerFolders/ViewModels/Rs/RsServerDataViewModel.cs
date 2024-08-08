using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DevExpress.Mvvm.Native;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

using RevitServerFolders.Utils;

namespace RevitServerFolders.ViewModels.Rs {
    internal sealed class RsServerDataViewModel : RsModelObjectViewModel {

        public RsServerDataViewModel(IServerClient serverClient) : base(serverClient) {
            Size = "Расчет размера";
            _serverClient.GetFolderInfoAsync("|")
                .ContinueWith(t => {
                    Size = Extensions.BytesToString(t.Result.Size);
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override string Name => _serverClient.ServerName;
        public override string FullName => $"{_serverClient.ServerName}/{_serverClient.ServerVersion}";

        public override bool HasChildren => true;

        protected override async Task<IEnumerable<RsModelObjectViewModel>> GetChildrenObjects() {
            FolderContents folderContents = await _serverClient.GetRootFolderContentsAsync(
                CancellationTokenSource.Token);
            return folderContents.Folders
                .Select(item => new RsFolderDataViewModel(_serverClient, item, folderContents))
                .ToArray();
        }
    }
}
