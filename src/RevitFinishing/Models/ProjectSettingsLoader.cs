using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Bim4Everyone;

namespace RevitFinishing.Models
{
    internal class ProjectSettingsLoader {
        private readonly Application _application;
        private readonly Document _document;
        private readonly ProjectParameters _projectParameters;
        private readonly IEnumerable<RevitParam> _parameters;

        public ProjectSettingsLoader(Application application, Document document) {
            _application = application;
            _document = document;

            _projectParameters = ProjectParameters.Create(_application);

            _parameters = new List<RevitParam>() {
                SharedParamsConfig.Instance.FinishingRoomName,
                SharedParamsConfig.Instance.FinishingRoomNumber,
                SharedParamsConfig.Instance.FinishingRoomNames,
                SharedParamsConfig.Instance.FinishingRoomNumbers,
                SharedParamsConfig.Instance.FinishingType,
                SharedParamsConfig.Instance.FloorFinishingOrder,
                SharedParamsConfig.Instance.CeilingFinishingOrder,
                SharedParamsConfig.Instance.WallFinishingOrder,
                SharedParamsConfig.Instance.BaseboardFinishingOrder,
                SharedParamsConfig.Instance.SizeLengthAdditional,
                SharedParamsConfig.Instance.SizeArea,
                SharedParamsConfig.Instance.SizeVolume
            };
        }

        public void CopyParameters() {
            _projectParameters.SetupRevitParams(_document, _parameters);
        }

        public void CopyKeySchedule() {
            _projectParameters.SetupSchedule(_document, false, KeySchedulesConfig.Instance.RoomsFinishing);
        }
    }
}
