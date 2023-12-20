using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.ViewModels;

namespace RevitRoomFinishing.ViewModels
{
    class RoomsByNameViewModel : BaseViewModel
    {
        private readonly string _name;
        private readonly IReadOnlyCollection<Room> _rooms;
        private bool _isChecked;

        public RoomsByNameViewModel(string name, IEnumerable<Room> rooms) {
            _rooms = rooms.ToList();
            _name = name;
        }

        public IReadOnlyCollection<Room> Rooms => _rooms;
        public string Name => _name;
        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);
        }
    }
}
