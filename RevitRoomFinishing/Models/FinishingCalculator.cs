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

namespace RevitRoomFinishing.Models
{
    class FinishingCalculator
    {
        private readonly List<Element> _revitRooms;
        private readonly Finishing _revitFinishings;
        private readonly List<FinishingElement> _finishings;
        private readonly List<ErrorElementInfo> _errorElements;
        private readonly List<ErrorElementInfo> _warningElements;
        private Dictionary<string, FinishingType> _roomsByFinishingType;

        public FinishingCalculator(IEnumerable<Element> rooms, Finishing finishings) {
            _revitRooms = rooms.ToList();
            _revitFinishings = finishings;

            _errorElements = new List<ErrorElementInfo>();
            _warningElements = new List<ErrorElementInfo>();

            _errorElements.AddRange(CheckFinishingByRoomBounding().Select(x => new ErrorElementInfo(x)));
            _errorElements.AddRange(CheckRoomsByKeyParameter("ОТД_Тип отделки").Select(x => new ErrorElementInfo(x)));

            if(!_errorElements.Any()) {
                _finishings = SetRoomsForFinishing();
                _errorElements.AddRange(CheckFinishingByRoom().Select(x => new ErrorElementInfo(x)));

                _warningElements.AddRange(CheckRoomsByParameter("Номер").Select(x => new ErrorElementInfo(x)));
                _warningElements.AddRange(CheckRoomsByParameter("Имя").Select(x => new ErrorElementInfo(x)));
            }
        }

        public List<FinishingElement> Finishings => _finishings;
        public Dictionary<string, FinishingType> RoomsByFinishingType => _roomsByFinishingType;
        public List<ErrorElementInfo> ErrorElements => _errorElements;
        public List<ErrorElementInfo> WarningElements => _warningElements;

        public List<Element> CheckFinishingByRoomBounding() {
            return _revitFinishings
                .AllFinishings
                .Where(x => x.GetParamValueOrDefault(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 0) == 1)
                .ToList();
        }

        public List<Element> CheckRoomsByKeyParameter(string paramName) {
            return _revitRooms
                .Where(x => x.GetParam(paramName).AsElementId() == ElementId.InvalidElementId)
                .ToList();
        }

        public List<Element> CheckRoomsByParameter(string paramName) {
            return _revitRooms
                .Where(x => x.GetParam(paramName) == null)
                .ToList();
        }

        public List<Element> CheckFinishingByRoom() {
            return _finishings
                .Where(x => !x.CheckFinishingTypes())
                .Select(x => x.RevitElement)
                .ToList();
        }

        public List<FinishingElement> SetRoomsForFinishing() {    
            List<RoomElement> finishingRooms = _revitRooms
                .OfType<Room>()
                .Select(x => new RoomElement(x, _revitFinishings))
                .ToList();

            Dictionary<ElementId, FinishingElement> allFinishings = new Dictionary<ElementId, FinishingElement>();

            foreach(var room in finishingRooms) {
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

            _roomsByFinishingType = finishingRooms
                .GroupBy(x => x.RoomFinishingType)
                .ToDictionary(x => x.Key, x => new FinishingType(x.ToList()));

            return allFinishings.Values.ToList();
        }
    }
}
