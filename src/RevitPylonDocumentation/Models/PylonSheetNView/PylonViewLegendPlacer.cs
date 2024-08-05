using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewLegendPlacer {
        internal PylonViewLegendPlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }




        /// <summary>
        /// Размещает легенду примечаний на листе над штампом
        /// </summary>
        internal bool PlaceNoteLegend() {

            // Проверям вдруг легенда не выбрана
            if(ViewModel.SelectedLegend is null) {
                return false;
            } else {

                // Заполняем данные для задания
                SheetInfo.LegendView.ViewportTypeName = "Без названия";

                double coordinateX = UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ProjectSettings.LegendXOffset));
                double coordinateY = UnitUtilsHelper.ConvertToInternalValue(int.Parse(ViewModel.ProjectSettings.LegendYOffset));

                SheetInfo.LegendView.ViewportCenter = new XYZ(coordinateX, coordinateY, 0);
            }


            // Проверяем можем ли разместить на листе легенду
            if(!Viewport.CanAddViewToSheet(Repository.Document, SheetInfo.PylonViewSheet.Id, ViewModel.SelectedLegend.Id)) { return false; }

            // Размещаем легенду на листе
            Viewport viewPort = Viewport.Create(Repository.Document, SheetInfo.PylonViewSheet.Id, ViewModel.SelectedLegend.Id, SheetInfo.LegendView.ViewportCenter);

            SheetInfo.LegendView.ViewportElement = viewPort;


            // Задание правильного типа видового экрана
            ICollection<ElementId> typesOfViewPort = viewPort.GetValidTypes();
            foreach(ElementId typeId in typesOfViewPort) {
                ElementType type = Repository.Document.GetElement(typeId) as ElementType;
                if(type == null) {
                    continue;
                }

                if(type.Name == SheetInfo.LegendView.ViewportTypeName) {
                    viewPort.ChangeTypeId(type.Id);
                    break;
                }
            }

            return true;
        }
    }
}
