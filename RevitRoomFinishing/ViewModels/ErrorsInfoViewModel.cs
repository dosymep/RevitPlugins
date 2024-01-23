using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitRoomFinishing.ViewModels {
    internal class ErrorsInfoViewModel : BaseViewModel {
        private ErrorsListViewModel _selectedList;

        public ObservableCollection<ErrorsListViewModel> ErrorLists { get; set; }

        public ErrorsListViewModel SelectedList {
            get => _selectedList;
            set => RaiseAndSetIfChanged(ref _selectedList, value);
        }
    }
}
