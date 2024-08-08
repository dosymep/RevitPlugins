using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

using RevitServerFolders.Models;
using RevitServerFolders.Models.Rs;
using RevitServerFolders.Utils;

namespace RevitServerFolders.ViewModels.Rs {
    internal sealed class RsFolderDataViewModel : RsModelObjectViewModel {
        private readonly FolderData _folderData;
        private readonly FolderContents _folderContents;

        public RsFolderDataViewModel(IServerClient serverClient, FolderData folderData, FolderContents folderContents) :
            base(serverClient) {
            _folderData = folderData;
            _folderContents = folderContents;

            Size = Extensions.BytesToString(_folderData.Size);
        }

        public override string Name => _folderData.Name;
        public override string FullName => _serverClient.GetVisibleModelPath(_folderContents, _folderData);

        public override bool HasChildren => _folderData.HasContents;

        protected override async Task<IEnumerable<RsModelObjectViewModel>> GetChildrenObjects() {
            string relativePath = _folderContents.GetRelativeModelPath(_folderData);
            FolderContents folderContents = await _serverClient.GetFolderContentsAsync(
                relativePath, CancellationTokenSource.Token);
            return folderContents.Folders
                .Select(item => new RsFolderDataViewModel(_serverClient, item, folderContents))
                .ToArray();
        }

        public override ModelObject GetModelObject() {
            return new RsFolderModel(_folderData, _folderContents, _serverClient);
        }
    }
}
