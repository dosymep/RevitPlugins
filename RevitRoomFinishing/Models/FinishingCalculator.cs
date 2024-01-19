using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

using RevitRoomFinishing.ViewModels;
using System.Web.Security;
using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitRoomFinishing.Models
{
    class FinishingCalculator
    {
        private readonly List<Element> _revitRooms;
        private readonly List<Element> _revitFinishings;
        private readonly List<FinishingElement> _finishings;

        public FinishingCalculator(IEnumerable<ElementsGroupViewModel> roomNames, IEnumerable<ElementsGroupViewModel> finishingTypes) {
            _revitRooms = roomNames
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Elements)
                .ToList();

            _revitFinishings = finishingTypes
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Elements)
                .ToList();

            _finishings = SetRoomsForFinishing();
        }

        public List<FinishingElement> Finishings => _finishings;

        public List<Element> CheckFinishingByRoomBounding() {
            return _revitFinishings
                .Where(x => x.GetParamValue<bool>(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING))
                .ToList();
        }

        public List<Element> CheckRoomsByKeyParameter(string paramName) {
            return _revitRooms
                .Where(x => string.IsNullOrEmpty(x.GetParam(paramName).AsValueString()))
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

        private List<FinishingElement> SetRoomsForFinishing() {    
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
                        var newFinishing = new FinishingElement(finishingRevitElement) {
                            Rooms = new List<RoomElement> { room }
                        };

                        allFinishings.Add(finishingElementId, newFinishing);
                    }
                }
            }

            return allFinishings.Values.ToList();
        }
    }
}
