using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitRoomFinishing.Models
{
    class RoomElement {
        private readonly Room _revitRoom;
        private readonly Document _document;
        private readonly ElementIntersectsSolidFilter _solidFilter;
        private readonly BoundingBoxIntersectsFilter _bbFilter;

        private readonly IReadOnlyCollection<Element> _allFinishing;

        private readonly List<string> _wallTypesByOrder;
        private readonly List<string> _floorTypesByOrder;
        private readonly List<string> _ceilingTypesByOrder;
        private readonly List<string> _baseboardTypesByOrder;

        private readonly List<ElementId> _elements;
        
        public RoomElement(Room room, List<Element> elements) {
            _revitRoom = room;
            _elements = elements.Select(x => x.Id).ToList();

            _document = room.Document;
            Solid roomSolid = room
                .ClosedShell
                .OfType<Solid>()
                .First();
            _solidFilter = new ElementIntersectsSolidFilter(roomSolid);

            BoundingBoxXYZ bbXYZ = roomSolid.GetBoundingBox();
            BoundingBoxXYZ transformedBB = bbXYZ.TransformBoundingBox(bbXYZ.Transform);
            Outline roomOutline = new Outline(transformedBB.Min, transformedBB.Max);
                _bbFilter = new BoundingBoxIntersectsFilter(roomOutline);

            IList<Element> _walls = GetElementsBySolidIntersection(BuiltInCategory.OST_Walls);
            IList<Element> _floors = GetElementsBySolidIntersection(BuiltInCategory.OST_Floors);
            IList<Element> _ceilings = GetElementsBySolidIntersection(BuiltInCategory.OST_Ceilings);
            IList<Element> _baseboards = GetElementsBySolidIntersection(BuiltInCategory.OST_Walls);

            _allFinishing = _walls
                .Concat(_floors)
                .Concat(_ceilings)
                .Concat(_baseboards)
                .ToList();

            _wallTypesByOrder = CalculateFinishingOrder(_walls);
            _floorTypesByOrder = CalculateFinishingOrder(_floors);
            _ceilingTypesByOrder = CalculateFinishingOrder(_ceilings);
            _baseboardTypesByOrder = CalculateFinishingOrder(_ceilings);
        }

        public Room RevitRoom => _revitRoom;
        public IReadOnlyCollection<Element> AllFinishing => _allFinishing;

        private List<string> CalculateFinishingOrder(IList<Element> finishingElements) {
            return finishingElements
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

        private IList<Element> GetElementsBySolidIntersection(BuiltInCategory category) {
            return new FilteredElementCollector(_document, _elements)
               .OfCategory(category)
               .WhereElementIsNotElementType()
               .WherePasses(_bbFilter)
               .WherePasses(_solidFilter)
               .ToElements();
        }
    }
}
