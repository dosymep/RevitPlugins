using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;
using RevitRooms.ViewModels.Revit;

namespace RevitRooms.ViewModels {
    internal class RoomsViewModel : BaseViewModel {
        private RevitViewModel _revitViewModel;

        public RoomsViewModel(Application application, Document document) {
            RevitViewModels = new ObservableCollection<RevitViewModel> {
                new ViewRevitViewModel(application, document) { Name = "Выборка по текущему виду" },
                new ElementsRevitViewModel(application, document) { Name = "Выборка по всем элементам" },
                new SelectedRevitViewModel(application, document) { Name = "Выборка по выделенным элементам" }
            };

            RevitViewModel = RevitViewModels[1];

            var roomsConfig = RoomsConfig.GetRoomsConfig();
            var settings = roomsConfig.GetSettings(document);
            if(settings != null) {
                RevitViewModel = RevitViewModels.FirstOrDefault(item => item._id == settings.SelectedRoomId) ?? RevitViewModel;
            }
        }

        public RevitViewModel RevitViewModel {
            get => _revitViewModel;
            set => this.RaiseAndSetIfChanged(ref _revitViewModel, value);
        }

        public ObservableCollection<RevitViewModel> RevitViewModels { get; }
    }
}
