using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using dosymep.Revit.ServerClient;
using dosymep.Revit.ServerClient.DataContracts;

using RevitServerFolders.Models;
using RevitServerFolders.Models.Rs;
using RevitServerFolders.ViewModels.Rs;
using RevitServerFolders.Views.Rs;

namespace RevitServerFolders.Services {
    internal sealed class RsModelObjectService : IModelObjectService {
        private readonly MainViewModel _mainViewModel;
        private readonly IReadOnlyCollection<IServerClient> _serverClients;

        public RsModelObjectService(MainViewModel mainViewModel, IReadOnlyCollection<IServerClient> serverClients) {
            _mainViewModel = mainViewModel;
            _serverClients = serverClients;
        }

        public Task<ModelObject> SelectModelObjectDialog() {
            return SelectModelObjectDialog(null);
        }

        public Task<ModelObject> SelectModelObjectDialog(string rootFolder) {
            _mainViewModel.RemoveCancellation();
            MainWindow window = new MainWindow() { DataContext = _mainViewModel, Title = "Выберите папку" };
            if(window.ShowDialog() == true) {
                return Task.FromResult(_mainViewModel.SelectedItem.GetModelObject());
            }
            _mainViewModel.CancelCommands();

            throw new OperationCanceledException();
        }

        public async Task<ModelObject> GetFromString(string folderName) {
            Uri uri = new Uri(folderName.Replace(@"\", @"/"));
            IServerClient serverClient = _serverClients.FirstOrDefault(
                item => item.ServerName.Equals(uri.Host, StringComparison.OrdinalIgnoreCase));
            if(serverClient == null) {
                throw new InvalidOperationException($"Не был найден сервер \"{uri.Host}\".");
            }

            string parent = Path.GetDirectoryName(uri.LocalPath);
            string currentFolder = Path.GetFileName(uri.LocalPath);

            FolderContents folderContents;
            if(string.IsNullOrEmpty(parent)) {
                folderContents = await serverClient.GetRootFolderContentsAsync();
            } else {
                folderContents = await serverClient.GetFolderContentsAsync(parent);
            }

            FolderData folderData = folderContents.Folders
                .FirstOrDefault(item => item.Name.Equals(currentFolder));

            if(folderData == null) {
                throw new InvalidOperationException($"Не была найдена папка \"{uri.LocalPath.Trim('\\').Trim('/')}\".");
            }

            return new RsFolderModel(folderData, folderContents, serverClient);
        }
    }
}
