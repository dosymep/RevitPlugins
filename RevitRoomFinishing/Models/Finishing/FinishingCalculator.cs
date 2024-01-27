using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

using RevitRoomFinishing.ViewModels;
using dosymep.Bim4Everyone;
using dosymep.Revit;
using RevitRoomFinishing.Views;

namespace RevitRoomFinishing.Models
{
    class FinishingCalculator
    {
        private readonly List<Element> _revitRooms;
        private readonly Finishing _revitFinishings;
        private readonly List<FinishingElement> _finishings;

        private readonly string _phaseName;

        private readonly List<RoomElement> _finishingRooms;

        private readonly ErrorsViewModel _errors;
        private readonly ErrorsViewModel _warnings;
        private readonly Dictionary<string, FinishingType> _roomsByFinishingType;

        public FinishingCalculator(IEnumerable<Element> rooms, Finishing finishings, Phase phase) {
            _revitRooms = rooms.ToList();
            _revitFinishings = finishings;
            _phaseName = phase.Name;

            _errors = new ErrorsViewModel();
            _warnings = new ErrorsViewModel();

            _errors.AddElements(new ErrorsListViewModel() {
                Message = "Ошибка",
                Description = "Экземпляры отделки являются границами помещений",
                Elements = new ObservableCollection<ErrorElement>(CheckFinishingByRoomBounding())
            });
            _errors.AddElements(new ErrorsListViewModel() {
                Message = "Ошибка",
                Description = "У помещений не заполнен ключевой параметр отделки",
                Elements = new ObservableCollection<ErrorElement>(CheckRoomsByKeyParameter("ОТД_Тип отделки"))
            });

            if(!_errors.ErrorLists.Any()) {
                _finishingRooms = _revitRooms
                    .OfType<Room>()
                    .Select(x => new RoomElement(x, _revitFinishings))
                    .ToList();
                _finishings = SetRoomsForFinishing();
                _roomsByFinishingType = GroupRoomsByFinishingType();

                _errors.AddElements(new ErrorsListViewModel() {
                    Message = "Ошибка",
                    Description = "Элементы отделки относятся к помещениям с разными типами отделки",
                    Elements = new ObservableCollection<ErrorElement>(CheckFinishingByRoom())
                });

                _warnings.AddElements(new ErrorsListViewModel() {
                    Message = "Предупреждение",
                    Description = "У помещений не заполнен параметр \"Номер\"",
                    Elements = new ObservableCollection<ErrorElement>(CheckRoomsByParameter("Номер"))
                });

                _warnings.AddElements(new ErrorsListViewModel() {
                    Message = "Предупреждение",
                    Description = "У помещений не заполнен параметр \"Имя\"",
                    Elements = new ObservableCollection<ErrorElement>(CheckRoomsByParameter("Имя"))
                });
            }
        }

        public List<FinishingElement> Finishings => _finishings;
        public Dictionary<string, FinishingType> RoomsByFinishingType => _roomsByFinishingType;
        
        public ErrorsViewModel ErrorElements => _errors;
        public ErrorsViewModel WarningElements => _warnings;

        public List<ErrorElement> CheckFinishingByRoomBounding() {
            return _revitFinishings
                .AllFinishings
                .Where(x => x.GetParamValueOrDefault(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 0) == 1)
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        private List<ErrorElement> CheckRoomsByKeyParameter(string paramName) {
            return _revitRooms
                .Where(x => x.GetParam(paramName).AsElementId() == ElementId.InvalidElementId)
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        private List<ErrorElement> CheckRoomsByParameter(string paramName) {
            return _revitRooms
                .Where(x => string.IsNullOrEmpty(x.GetParamValue<string>(paramName)))
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        private List<ErrorElement> CheckFinishingByRoom() {
            return _finishings
                .Where(x => !x.CheckFinishingTypes())
                .Select(x => x.RevitElement)
                .Select(x => new ErrorElement(x, _phaseName))
                .ToList();
        }

        private List<FinishingElement> SetRoomsForFinishing() {    
            Dictionary<ElementId, FinishingElement> allFinishings = new Dictionary<ElementId, FinishingElement>();

            foreach(var room in _finishingRooms) {
                foreach(var finishingRevitElement in room.AllFinishing) {
                    ElementId finishingElementId = finishingRevitElement.Id;
                    if(allFinishings.TryGetValue(finishingElementId, out FinishingElement elementInDict)) {
                        elementInDict.Rooms.Add(room);
                    } 
                    else {
                        var newFinishing = new FinishingElement(finishingRevitElement, this) {
                            Rooms = new List<RoomElement> { room }
                        };

                        allFinishings.Add(finishingElementId, newFinishing);
                    }
                }
            }
            return allFinishings.Values.ToList();
        }

        private Dictionary<string, FinishingType> GroupRoomsByFinishingType() {
            return _finishingRooms
                .GroupBy(x => x.RoomFinishingType)
                .ToDictionary(x => x.Key, x => new FinishingType(x.ToList()));
        }
    }
}
