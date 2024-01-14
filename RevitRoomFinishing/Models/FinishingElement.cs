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
        private readonly Element _revitElement;
        
        public FinishingElement(Element element) {
            _revitElement = element;
        }
        public Element Element => _revitElement;

        public List<RoomElement> Rooms { get; set; }
        public int FinishingNumber => Rooms.First().GetFinishingOrder(_revitElement.Name);

        private string GetRoomsStrParameters(string parameterName) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParamValueOrDefault(parameterName, "123"))
                .Distinct();

            return string.Join("; ", values);
        }

        private string GetRoomsKeyParameters(string parameterName) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParam(parameterName).AsValueString())
                .Distinct();

            return string.Join("; ", values);
        }

        public void UpdateFinishingParameters() {
            _revitElement.SetParamValue("ФОП_ОТД_Полы Тип 1", GetRoomsStrParameters("ФОП_ОТД_Полы Тип 1"));
            _revitElement.SetParamValue("ФОП_ОТД_Полы Тип 2", GetRoomsStrParameters("ФОП_ОТД_Полы Тип 2"));
            _revitElement.SetParamValue("ФОП_ОТД_Потолки Тип 1", GetRoomsStrParameters("ФОП_ОТД_Потолки Тип 1"));
            _revitElement.SetParamValue("ФОП_ОТД_Потолки Тип 2", GetRoomsStrParameters("ФОП_ОТД_Потолки Тип 2"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 1", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 1"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 2", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 2"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 3", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 3"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 4", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 4"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 5", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 5"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 6", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 6"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 7", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 7"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 8", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 8"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 9", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 9"));
            _revitElement.SetParamValue("ФОП_ОТД_Стены Тип 10", GetRoomsStrParameters("ФОП_ОТД_Стены Тип 10"));
            _revitElement.SetParamValue("ФОП_ОТД_Плинтусы Тип 1", GetRoomsStrParameters("ФОП_ОТД_Плинтусы Тип 1"));
            _revitElement.SetParamValue("ФОП_ОТД_Имя помещения", GetRoomsStrParameters("Имя"));
            _revitElement.SetParamValue("ФОП_ОТД_Номер помещения", GetRoomsStrParameters("Номер"));
            _revitElement.SetParamValue("ФОП_ОТД_Тип отделки_ТЕ", GetRoomsKeyParameters("ОТД_Тип отделки"));

            if(_revitElement.Category.Id == Category.GetCategory(_revitElement.Document, BuiltInCategory.OST_Walls).Id) {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Длина"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип стены_ДЕ", FinishingNumber);
            }
            else if(_revitElement.Category.Id == Category.GetCategory(_revitElement.Document, BuiltInCategory.OST_Floors).Id) {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Периметр"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип пола_ДЕ", FinishingNumber);
            } 
            else {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Периметр"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип потолка_ДЕ", FinishingNumber);
            }
            
            _revitElement.SetParamValue("ФОП_РАЗМ_Площадь", _revitElement.GetParamValue<double>("Площадь"));
            _revitElement.SetParamValue("ФОП_РАЗМ_Объем", _revitElement.GetParamValue<double>("Объем"));
        }
    }
}
