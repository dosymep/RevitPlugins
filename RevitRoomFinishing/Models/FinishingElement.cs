using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using pyRevitLabs.Json.Linq;

namespace RevitRoomFinishing.Models
{
    class FinishingElement
    {
        private readonly Element _revitElement;
        private readonly FinishingCalculator _calculator;
        
        public FinishingElement(Element element, FinishingCalculator calculator) {
            _revitElement = element;
            _calculator = calculator;
        }

        public Element RevitElement => _revitElement;
        public List<RoomElement> Rooms { get; set; }

        private string GetRoomsStrParameters(string parameterName) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParamValue<string>(parameterName))
                .Distinct();

            return string.Join("; ", values);
        }

        private string GetRoomsKeyParameters(string parameterName) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParam(parameterName).AsValueString())
                .Distinct();

            return string.Join("; ", values);
        }

        public bool CheckFinishingTypes() {
            List<string> finishingTypes = Rooms
                .Select(x => x.RevitRoom)
                //.Select(x => x.GetParamValueString("ОТД_Тип отделки"))
                .Select(x => x.GetParam("ОТД_Тип отделки").AsValueString())
                .Distinct()
                .ToList();

            if(finishingTypes.Count == 1)
                return true;
            return false;
        }

        public void UpdateFinishingParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

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
            _revitElement.SetParamValue("ФОП_ОТД_Плинтусы Тип 2", GetRoomsStrParameters("ФОП_ОТД_Плинтусы Тип 2"));

            _revitElement.SetParamValue("ФОП_ОТД_Имя помещения", GetRoomsStrParameters("Имя"));
            _revitElement.SetParamValue("ФОП_ОТД_Номер помещения", GetRoomsStrParameters("Номер"));

            _revitElement.SetParamValue("ФОП_ОТД_Тип отделки_ТЕ", GetRoomsKeyParameters("ОТД_Тип отделки"));
            
            _revitElement.SetParamValue("ФОП_РАЗМ_Площадь", _revitElement.GetParamValue<double>("Площадь"));
            _revitElement.SetParamValue("ФОП_РАЗМ_Объем", _revitElement.GetParamValue<double>("Объем"));

            if(_revitElement.Name.Contains(FinishingCategory.Walls.KeyWord)) {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Длина"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип стены_ДЕ", finishingType.GetWallOrder(_revitElement.Name));
            }
            else if(_revitElement.Name.Contains(FinishingCategory.Baseboards.KeyWord)) {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Длина"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип плинтуса_ДЕ", finishingType.GetBaseboardOrder(_revitElement.Name));
            } 
            else if(_revitElement.Name.Contains(FinishingCategory.Floors.KeyWord)) {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Периметр"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип пола_ДЕ", finishingType.GetFloorOrder(_revitElement.Name));
            } 
            else {
                _revitElement.SetParamValue("ФОП_РАЗМ_Длина_ДЕ", _revitElement.GetParamValue<double>("Периметр"));
                _revitElement.SetParamValue("ФОП_ОТД_Тип потолка_ДЕ", finishingType.GetCeilingOrder(_revitElement.Name));
            }
        }
    }
}
