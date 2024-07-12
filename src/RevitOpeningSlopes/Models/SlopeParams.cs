using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class SlopeParams {
        private readonly RevitRepository _revitRepository;

        public SlopeParams(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public void SetSlopeParams(FamilyInstance slope, SlopeCreationData slopeCreationData) {
            ElementExtensions.SetParamValue(slope, "Высота", slopeCreationData.Height);
            ElementExtensions.SetParamValue(slope, "Ширина", slopeCreationData.Width);
            ElementExtensions.SetParamValue(slope, "Глубина", slopeCreationData.Depth);

            //Поворот элемента
            double rotationAngle = slopeCreationData.RotationRadiansAngle;
            Transform openingTransform = slope.GetTotalTransform();
            Line middleLine = Line.CreateUnbound(slopeCreationData.Center, openingTransform.BasisZ);
            ElementTransformUtils.RotateElement(_revitRepository.Document, slope.Id, middleLine, rotationAngle);
        }
    }
}
