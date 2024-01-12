using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;

namespace RevitRoomFinishing.Models
{
    class RoomElement {
        Room _room;

        public RoomElement(Room room) {
            _room = room;
        }

        public Room RevitRoom => _room;

        public List<Element> GetBoundaryWalls() {
            ElementId wallCategoryId = new ElementId(BuiltInCategory.OST_Walls);

            return _room.GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions)
                .SelectMany(x => x)
                .Select(x => x.ElementId)
                .Where(x => x.IsNotNull())
                .Select(x => _room.Document.GetElement(x))
                .Where(x => x.Category?.Id == wallCategoryId)
                .ToList();
        }

        public List<Element> GetFloors() {
            return new List<Element>();
        }

        public List<Element> GetCeilings() {
            return new List<Element>();
        }
    }
}
