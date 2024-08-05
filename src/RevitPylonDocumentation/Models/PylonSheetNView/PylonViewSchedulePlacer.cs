using System;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewSchedulePlacer {
        internal PylonViewSchedulePlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }



        internal bool PlaceRebarSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.RebarSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.RebarSchedule.ViewportName =
                    ViewModel.SchedulesSettings.RebarSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.RebarScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.RebarSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации арматуры пилона
            XYZ newCenter = new XYZ(
                -SheetInfo.RebarSchedule.ViewportHalfWidth * 2 - 0.0095,
                SheetInfo.TitleBlockHeight - 0.032,
                0);
            (SheetInfo.RebarSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.RebarSchedule.ViewportCenter = newCenter;

            return true;
        }





        internal bool PlaceMaterialSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.MaterialSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.MaterialSchedule.ViewportName =
                    ViewModel.SchedulesSettings.MaterialSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.MaterialScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.MaterialSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации материалов пилона
            XYZ newCenter = new XYZ(
                    -SheetInfo.MaterialSchedule.ViewportHalfWidth * 2 - 0.0095,
                    SheetInfo.TitleBlockHeight - SheetInfo.RebarSchedule.ViewportHalfHeight * 2 - 0.02505,
                    0);


            if(SheetInfo.RebarSchedule.ViewportElement is null) {
                newCenter = new XYZ(
                    -SheetInfo.MaterialSchedule.ViewportHalfWidth * 2 - 0.0095,
                    -0.1,
                    0);
            }


            (SheetInfo.MaterialSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.MaterialSchedule.ViewportCenter = newCenter;

            return true;
        }



        internal bool PlaceSystemPartsSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.SystemPartsSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.SystemPartsSchedule.ViewportName =
                    ViewModel.SchedulesSettings.SystemPartsSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.SystemPartsScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.SystemPartsSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации системных деталей пилона
            XYZ newCenter = new XYZ(
                    0,
                    SheetInfo.TitleBlockHeight / 2 + SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2,
                    0);

            if(SheetInfo.IFCPartsSchedule.ViewportElement is null) {

                newCenter = new XYZ(
                    -UnitUtilsHelper.ConvertToInternalValue(2.9) - SheetInfo.SystemPartsSchedule.ViewportHalfWidth * 2,
                    SheetInfo.TitleBlockHeight / 2 + SheetInfo.SystemPartsSchedule.ViewportHalfHeight * 2,
                    0);
            }

            (SheetInfo.SystemPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.SystemPartsSchedule.ViewportCenter = newCenter;

            return true;
        }


        internal bool PlaceIFCPartsSchedule() {

            // Проверям вдруг спека не создалась
            if(SheetInfo.IFCPartsSchedule.ViewElement == null) {
                return false;
            } else {

                // Заполнеяем данные для задания
                SheetInfo.IFCPartsSchedule.ViewportName =
                    ViewModel.SchedulesSettings.IFCPartsSchedulePrefix
                        + SheetInfo.PylonKeyName
                        + ViewModel.SchedulesSettings.IFCPartsScheduleSuffix;
            }

            // Передаем спеку армирования в метод по созданию видовых экранов в (0.0.0)
            if(!PlaceScheduleViewport(SheetInfo.PylonViewSheet, SheetInfo.IFCPartsSchedule)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки спецификации системных деталей пилона
            XYZ newCenter = new XYZ(
                    0,
                    SheetInfo.TitleBlockHeight / 2 + SheetInfo.IFCPartsSchedule.ViewportHalfHeight * 2,
                    0);

            if(SheetInfo.SystemPartsSchedule.ViewportElement is null) {

                newCenter = new XYZ(
                    -UnitUtilsHelper.ConvertToInternalValue(2.9) - SheetInfo.IFCPartsSchedule.ViewportHalfWidth * 2,
                    SheetInfo.TitleBlockHeight / 2 + SheetInfo.IFCPartsSchedule.ViewportHalfHeight * 2,
                    0);
            }


            (SheetInfo.IFCPartsSchedule.ViewportElement as ScheduleSheetInstance).Point = newCenter;

            SheetInfo.IFCPartsSchedule.ViewportCenter = newCenter;

            return true;
        }





        public bool PlaceScheduleViewport(ViewSheet viewSheet, PylonView pylonView) {
            Document doc = Repository.Document;

            ScheduleSheetInstance scheduleSheetInstance = null;
            // Размещаем спеку пилона на листе
            try {
                scheduleSheetInstance = ScheduleSheetInstance.Create(doc, viewSheet.Id, pylonView.ViewElement.Id, new XYZ(0, 0, 0));

            } catch(Exception) {
                return false;
            }

            pylonView.ViewportElement = scheduleSheetInstance;


            // Получение габаритов видового экрана спецификации
            GetScheduleViewportInfo(viewSheet, pylonView, scheduleSheetInstance);

            return true;
        }


        /// <summary>
        /// Получает и запоминает точку вставки спецификации и ее габариты
        /// </summary>
        public void GetScheduleViewportInfo(ViewSheet viewSheet, PylonView pylonView, ScheduleSheetInstance viewport) {

            XYZ viewportCenter = viewport.Point;

            // Точка вставки спеки в верхнем левый угол спеки
            BoundingBoxXYZ boundingBoxXYZ = viewport.get_BoundingBox(viewSheet);
            double viewportHalfWidth = (boundingBoxXYZ.Max.X - viewportCenter.X) / 2;
            double viewportHalfHeight = (viewportCenter.Y - boundingBoxXYZ.Min.Y) / 2;

            // Запись центра и габаритов видового экрана спецификации
            pylonView.ViewportCenter = viewportCenter;
            pylonView.ViewportHalfWidth = viewportHalfWidth;
            pylonView.ViewportHalfHeight = viewportHalfHeight;
        }


    }
}
