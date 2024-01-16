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
        private readonly List<Element> _baseboards;
        private readonly List<Element> _allFinishing;
        private List<string> _wallTypesByOrder;
        private List<string> _floorTypesByOrder;
        private List<string> _ceilingTypesByOrder;
        private List<string> _baseboardTypesByOrder;


        public RoomElement(Room room) {
            _room = room;
            _walls = GetBoundaryWalls();
            _floors = GetFloors();
            _ceilings = GetCeilings();
            _baseboards = GetBaseboards();

            _allFinishing = new List<Element>();

            _wallTypesByOrder = CalculateFinishingOrder(_walls);
            _floorTypesByOrder = CalculateFinishingOrder(_floors);
            _ceilingTypesByOrder = CalculateFinishingOrder(_ceilings);
            _baseboardTypesByOrder = CalculateFinishingOrder(_ceilings);
        }

        public Room RevitRoom => _room;
        public List<Element> Walls => _walls;
        public List<Element> Floors => _floors;
        public List<Element> Ceilings => _ceilings;
        public List<Element> Baseboards => _baseboards;
        public List<Element> AllFinishing => _allFinishing;


        private List<string> CalculateFinishingOrder(List<Element> roomElements) {
            return roomElements
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public int GetFinishingWallOrder(string typeName) {
            return _wallTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetFinishingFloorOrder(string typeName) {
            return _floorTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetFinishinCeilingOrder(string typeName) {
            return _ceilingTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetFinishinBaseboardsOrder(string typeName) {
            return _baseboardTypesByOrder.IndexOf(typeName) + 1;
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

        public List<Element> GetBaseboards() {
            return new List<Element>();
        }
    }
}
