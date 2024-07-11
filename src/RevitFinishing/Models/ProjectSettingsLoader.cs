using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Bim4Everyone;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace RevitFinishing.Models
{
    internal class ProjectSettingsLoader {
        private readonly Application _application;
        private readonly Document _document;
        private readonly IEnumerable<RevitParam> _parameters;

        public ProjectSettingsLoader(Application application, Document document) {
            _application = application;
            _document = document;

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
            ProjectParameters
                .Create(_application)
                .SetupRevitParams(_document, _parameters);
        }

        public void CopyKeySchedule() {
            ProjectParameters
                .Create(_application)
                .SetupSchedule(_document, false, KeySchedulesConfig.Instance.RoomsFinishing);
        }
    }
}
