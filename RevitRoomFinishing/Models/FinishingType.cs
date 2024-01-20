using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AdvanceSteel.Modelling;
using Autodesk.Revit.DB;

namespace RevitRoomFinishing.Models {
    internal class FinishingType {
        private readonly List<RoomElement> _rooms;

        private readonly List<string> _wallTypesByOrder;
        private readonly List<string> _floorTypesByOrder;
        private readonly List<string> _ceilingTypesByOrder;
        private readonly List<string> _baseboardTypesByOrder;

        public FinishingType(List<RoomElement> rooms) {
            _rooms = rooms;

            IList<Element> _walls = _rooms.SelectMany(x => x.Walls).ToList();
            IList<Element> _floors = _rooms.SelectMany(x => x.Floors).ToList();
            IList<Element> _ceilings = _rooms.SelectMany(x => x.Ceilings).ToList();
            IList<Element> _baseboards = _rooms.SelectMany(x => x.Baseboards).ToList();

            _wallTypesByOrder = CalculateFinishingOrder(_walls);
            _floorTypesByOrder = CalculateFinishingOrder(_floors);
            _ceilingTypesByOrder = CalculateFinishingOrder(_ceilings);
            _baseboardTypesByOrder = CalculateFinishingOrder(_ceilings);
        }

        public int GetWallOrder(string typeName) {
            return _wallTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetFloorOrder(string typeName) {
            return _floorTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetCeilingOrder(string typeName) {
            return _ceilingTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetBaseboardOrder(string typeName) {
            return _baseboardTypesByOrder.IndexOf(typeName) + 1;
        }

        private List<string> CalculateFinishingOrder(IList<Element> finishingElements) {
            return finishingElements
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}
