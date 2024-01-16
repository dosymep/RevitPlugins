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
        private readonly List<string> _selectedFinishings;

        public FinishingCalculator(IEnumerable<ElementsGroupViewModel> roomNames, IEnumerable<ElementsGroupViewModel> finishingTypes) {
            _rooms = roomNames.ToList();
            _selectedFinishings = GetSelectedFinishing(finishingTypes);
            _finishings = SetRoomsForFinishing();
        }

        public List<FinishingElement> Finishings => _finishings;

        private List<string> GetSelectedFinishing(IEnumerable<ElementsGroupViewModel> finishingTypes) {
            return finishingTypes
                .Where(x => x.IsChecked)
                .Select(x => x.Name)
                .ToList();
        }

        private List<FinishingElement> SetRoomsForFinishing() {
            List<Element> selectedRooms = _rooms
                .Where(x => x.IsChecked)
                .SelectMany(x => x.Elements)
                .ToList();

            List<RoomElement> finishingRooms = selectedRooms
                .OfType<Room>()
                .Select(x => new RoomElement(x))
                .ToList();

            Dictionary<ElementId, FinishingElement> allFinishings = new Dictionary<ElementId, FinishingElement>();

            foreach(var room in finishingRooms) {
                var finishings = room.Walls.Where(x => _selectedFinishings.Contains(x.Name));
                foreach(var finishingRevitElement in finishings) {
                    ElementId finishingElementId = finishingRevitElement.Id;
                    if(allFinishings.TryGetValue(finishingElementId, out FinishingElement elementInDict)) {
                        elementInDict.Rooms.Add(room);
                    } 
                    else {
                        var newWall = new FinishingElement(finishingRevitElement) {
                            Rooms = new List<RoomElement> { room }
                        };

                        allFinishings.Add(finishingElementId, newWall);
                    }
                }
            }

            return allFinishings.Values.ToList();
        }
    }
}
