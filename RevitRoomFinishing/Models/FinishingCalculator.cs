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

namespace RevitRoomFinishing.Models
{
    class FinishingCalculator
    {
        private readonly List<FinishingElement> _finishings;
        private readonly List<ElementsGroupViewModel> _rooms;
        private readonly List<ElementId> _selectedFinishingElements;

        public FinishingCalculator(IEnumerable<ElementsGroupViewModel> roomNames, IEnumerable<ElementsGroupViewModel> finishingTypes) {
            _rooms = roomNames.ToList();
            _selectedFinishingElements = GetSelectedFinishingElements(finishingTypes);
            _finishings = SetRoomsForFinishing();
        }

        public List<FinishingElement> Finishings => _finishings;

        private List<ElementId> GetSelectedFinishingElements(IEnumerable<ElementsGroupViewModel> finishingTypes) {
            return finishingTypes
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Elements)
                .Select(x => x.Id)
                .ToList();
        }

        public List<Element> CheckFinishingByRoom() {
            return _finishings
                .Where(x => x.CheckFinishingTypes())
                .Select(x => x.RevitElement)
                .ToList();
        }

        public List<Element> CheckFinishingByRoomBounding() {
            return new List<Element>();
        }

        public List<Element> CheckRoomsByKey() {
            return new List<Element>();
        }

        public List<Element> CheckRoomsByParameters() {
            return new List<Element>();
        }

        private List<FinishingElement> SetRoomsForFinishing() {
            List<Element> selectedRooms = _rooms
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Elements)
                .ToList();

            List<RoomElement> finishingRooms = selectedRooms
                .OfType<Room>()
                .Select(x => new RoomElement(x, _selectedFinishingElements))
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
