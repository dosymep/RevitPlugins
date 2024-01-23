using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitRoomFinishing.ViewModels {
    internal class ErrorsViewModel : BaseViewModel {
        private ErrorsListViewModel _selectedList;
        private ObservableCollection<ErrorsListViewModel> _errorLists;

        public ErrorsViewModel() {
            _errorLists = new ObservableCollection<ErrorsListViewModel>();
        }

        public ObservableCollection<ErrorsListViewModel> ErrorLists => _errorLists;

        public void AddElements(ErrorsListViewModel errorsList) {
            if(errorsList.Elements.Any()) {
                _errorLists.Add(errorsList);
            }
        }

        public ErrorsListViewModel SelectedList {
            get => _selectedList;
            set => RaiseAndSetIfChanged(ref _selectedList, value);
        }
    }
}
