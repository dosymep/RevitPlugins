using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitRoomFinishing.Models
{
    /// <summary>
    /// Класс для экземпляра отделки.
    /// Каждый элемент отделки хранит список всех помещений, к которым он относится.
    /// </summary>
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

        private string GetRoomsParameters(RevitParam parameter) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParamValue<string>(parameter))
                .Distinct();

            return string.Join("; ", values);
        }

        private string GetRoomsParameters(BuiltInParameter bltnParam) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParamValue<string>(bltnParam))
                .Distinct();

            return string.Join("; ", values);
        }

        private string GetRoomsKeyParameters(RevitParam parameter) {
            IEnumerable<string> values = Rooms
                .Select(x => x.RevitRoom.GetParam(parameter).AsValueString())
                .Distinct();

            return string.Join("; ", values);
        }

        /// <summary>
        /// Проверка типов отделки помещений.
        /// Все помещения, к которым относятся экземпляр отделки должны иметь одинаковый тип отделки.
        /// </summary>
        /// <returns></returns>
        public bool CheckFinishingTypes() {
            List<string> finishingTypes = Rooms
                .Select(x => x.RoomFinishingType)
                .Distinct()
                .ToList();

            if(finishingTypes.Count == 1)
                return true;
            return false;
        }

        public void UpdateFinishingParameters() {
            FinishingType finishingType = _calculator.RoomsByFinishingType[Rooms.First().RoomFinishingType];

            SharedParamsConfig paramConfig = SharedParamsConfig.Instance;

            _revitElement.SetParamValue(paramConfig.FloorFinishingType1, 
                                        GetRoomsParameters(paramConfig.FloorFinishingType1));
            _revitElement.SetParamValue(paramConfig.FloorFinishingType2, 
                                        GetRoomsParameters(paramConfig.FloorFinishingType2));
            _revitElement.SetParamValue(paramConfig.CeilingFinishingType1,
                                        GetRoomsParameters(paramConfig.CeilingFinishingType1));
            _revitElement.SetParamValue(paramConfig.CeilingFinishingType2, 
                                        GetRoomsParameters(paramConfig.CeilingFinishingType2));
            _revitElement.SetParamValue(paramConfig.WallFinishingType1, 
                                        GetRoomsParameters(paramConfig.WallFinishingType1));
            _revitElement.SetParamValue(paramConfig.WallFinishingType2, 
                                        GetRoomsParameters(paramConfig.WallFinishingType2));
            _revitElement.SetParamValue(paramConfig.WallFinishingType3, 
                                        GetRoomsParameters(paramConfig.WallFinishingType3));
            _revitElement.SetParamValue(paramConfig.WallFinishingType4, 
                                        GetRoomsParameters(paramConfig.WallFinishingType4));
            _revitElement.SetParamValue(paramConfig.WallFinishingType5, 
                                        GetRoomsParameters(paramConfig.WallFinishingType5));
            _revitElement.SetParamValue(paramConfig.WallFinishingType6, 
                                        GetRoomsParameters(paramConfig.WallFinishingType6));
            _revitElement.SetParamValue(paramConfig.WallFinishingType7, 
                                        GetRoomsParameters(paramConfig.WallFinishingType7));
            _revitElement.SetParamValue(paramConfig.WallFinishingType8, 
                                        GetRoomsParameters(paramConfig.WallFinishingType8));
            _revitElement.SetParamValue(paramConfig.WallFinishingType9, 
                                        GetRoomsParameters(paramConfig.WallFinishingType9));
            _revitElement.SetParamValue(paramConfig.WallFinishingType10, 
                                        GetRoomsParameters(paramConfig.WallFinishingType10));
            _revitElement.SetParamValue(paramConfig.BaseboardFinishingType1, 
                                        GetRoomsParameters(paramConfig.BaseboardFinishingType1));
            _revitElement.SetParamValue(paramConfig.BaseboardFinishingType2, 
                                        GetRoomsParameters(paramConfig.BaseboardFinishingType2));

            _revitElement.SetParamValue(paramConfig.FinishingRoomName, 
                                        GetRoomsParameters(BuiltInParameter.ROOM_NAME));
            _revitElement.SetParamValue(paramConfig.FinishingRoomNumber, 
                                        GetRoomsParameters(BuiltInParameter.ROOM_NUMBER));
            _revitElement.SetParamValue(paramConfig.FinishingRoomNames, 
                                        finishingType.GetRoomsParameters(BuiltInParameter.ROOM_NAME));
            _revitElement.SetParamValue(paramConfig.FinishingRoomNumbers, 
                                        finishingType.GetRoomsParameters(BuiltInParameter.ROOM_NUMBER));

            _revitElement.SetParamValue(paramConfig.FinishingType, 
                                        GetRoomsKeyParameters(ProjectParamsConfig.Instance.RoomFinishingType));
            
            _revitElement.SetParamValue(paramConfig.SizeArea, 
                                        _revitElement.GetParamValue<double>(BuiltInParameter.HOST_AREA_COMPUTED));
            _revitElement.SetParamValue(paramConfig.SizeVolume, 
                                        _revitElement.GetParamValue<double>(BuiltInParameter.HOST_VOLUME_COMPUTED));

            if(_revitElement.Name.Contains(FinishingCategory.Walls.KeyWord)) {
                _revitElement.SetParamValue(paramConfig.SizeLengthAdditional, 
                                            _revitElement.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                _revitElement.SetParamValue(paramConfig.WallFinishingOrder, 
                                            finishingType.GetWallOrder(_revitElement.Name));
            }
            else if(_revitElement.Name.Contains(FinishingCategory.Baseboards.KeyWord)) {
                _revitElement.SetParamValue(paramConfig.SizeLengthAdditional, 
                                            _revitElement.GetParamValue<double>(BuiltInParameter.CURVE_ELEM_LENGTH));
                _revitElement.SetParamValue(paramConfig.BaseboardFinishingOrder, 
                                            finishingType.GetBaseboardOrder(_revitElement.Name));
            } 
            else if(_revitElement.Name.Contains(FinishingCategory.Floors.KeyWord)) {
                _revitElement.SetParamValue(paramConfig.SizeLengthAdditional, 
                                            _revitElement.GetParamValue<double>(BuiltInParameter.HOST_PERIMETER_COMPUTED));
                _revitElement.SetParamValue(paramConfig.FloorFinishingOrder, 
                                            finishingType.GetFloorOrder(_revitElement.Name));
            } 
            else if(_revitElement.Name.Contains(FinishingCategory.Ceilings.KeyWord)) {
                _revitElement.SetParamValue(paramConfig.SizeLengthAdditional, 
                                            _revitElement.GetParamValue<double>(BuiltInParameter.HOST_PERIMETER_COMPUTED));
                _revitElement.SetParamValue(paramConfig.CeilingFinishingOrder, 
                                            finishingType.GetCeilingOrder(_revitElement.Name));
            }
        }
    }
}
