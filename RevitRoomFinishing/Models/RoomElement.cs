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
        private readonly Room _room;
        private readonly List<Element> _walls;
        private readonly List<Element> _floors;
        private readonly List<Element> _ceilings;
        private List<string> _wallTypes;


        public RoomElement(Room room) {
            _room = room;
            _walls = GetBoundaryWalls();
            _floors = GetFloors();
            _ceilings = GetCeilings();

            CalculateFinishingOrder();
        }

        public Room RevitRoom => _room;
        public List<Element> Walls => _walls;
        public List<Element> Floors => _floors;
        public List<Element> Ceilings => _ceilings;

        public void CalculateFinishingOrder() {
            _wallTypes = _walls
                    .Select(x => x.Name)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
        }

        public int GetFinishingOrder(string typeName) {
            return _wallTypes.IndexOf(typeName) + 1;
        }

        private List<Element> GetBoundaryWalls() {
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
