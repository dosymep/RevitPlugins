using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitRoomFinishing.Models
{
    class FinishingElement
    {
        private Element _element;
        
        public FinishingElement(Element element) {
            _element = element;
        }
        public Element Element => _element;

        public List<RoomElement> Rooms { get; set; }

        public void UpdateFinishingParameters() {

        }
    }
}
