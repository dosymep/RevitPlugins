using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitRoomFinishing.Models {
    internal class Finishing {
        public ICollection<Element> Walls { get; set; }
        public ICollection<Element> Floors { get; set; }
        public ICollection<Element> Ceilings { get; set; }
        public ICollection<Element> Baseboards { get; set; }

        public ICollection<Element> AllFinishings => Walls
            .Concat(Baseboards)
            .Concat(Ceilings)
            .Concat(Floors)
            .ToList();
    }
}
