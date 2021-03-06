using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal class DoorViewModel : ElementViewModel<FamilyInstance> {
        public DoorViewModel(FamilyInstance door, PhaseViewModel phase, RevitRepository revitRepository) :
            base(door, revitRepository) {

            Phase = phase;
            try {
                ToRoom = new SpatialElementViewModel(Element.get_ToRoom(Phase.Element), revitRepository);
            } catch { }

            try {
                FromRoom = new SpatialElementViewModel(Element.get_FromRoom(Phase.Element), revitRepository);
            } catch { }
        }

        public PhaseViewModel Phase { get; }

        public SpatialElementViewModel ToRoom { get; }
        public SpatialElementViewModel FromRoom { get; }

        public ElementId LevelId => Element.LevelId;
        public override string LevelName => RevitRepository.GetElement(Element.LevelId)?.Name;

        public bool IsSectionNameEqual {
            get {
                if(ToRoom == null || FromRoom == null) {
                    return true;
                }

                return ToRoom.RoomSection?.Id == FromRoom.RoomSection?.Id;
            }
        }
    }
}
