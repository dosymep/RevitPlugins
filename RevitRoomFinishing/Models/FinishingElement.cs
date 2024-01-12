using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;

using pyRevitLabs.Json.Linq;

namespace RevitRoomFinishing.Models
{
    class FinishingElement
    {
        private Element _revitElement;
        
        public FinishingElement(Element element) {
            _revitElement = element;
        }
        public Element Element => _revitElement;

        public List<RoomElement> Rooms { get; set; }

        private string GetRoomsParameters(string parameterName) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParamValue<string>(parameterName))
                .Distinct();

            return string.Join("; ", values);
        }

        public void UpdateFinishingParameters() {
            string paramName = "ФОП_ОТД_Полы Тип 1";
            _revitElement.SetParamValue(paramName, GetRoomsParameters(paramName));
        }
    }
}
