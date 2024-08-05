using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewSectionPlacer {
        internal PylonViewSectionPlacer(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }



        // Метод для размещения основного вида пилона.
        // Позиционирование - левый верхний угол листа
        internal bool PlaceGeneralViewport() {

            // Проверям вдруг вид не создался
            if(SheetInfo.GeneralView.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.GeneralView.ViewportTypeName = "Заголовок на листе";
                SheetInfo.GeneralView.ViewportNumber = "100";
                SheetInfo.GeneralView.ViewportName =
                    ViewModel.ViewSectionSettings.GeneralViewPrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.ViewSectionSettings.GeneralViewSuffix;
            }

            // Передаем основной вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralView)) {
                return false;
            }

            // Если высота видового экрана основного вида больше, чем высота рамки, то он не поместится - меняем рамку
            if(SheetInfo.GeneralView.ViewportHalfHeight * 2 > SheetInfo.TitleBlockHeight) {
                SheetInfo.SetTitleBlockSize(Repository.Document, 2, 1);
            }

            double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralView.ViewportHalfWidth + 0.065;

            // Рассчитываем и задаем корректную точку вставки основного вида пилон, если есть еще и перпендикулярный
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                newCenterX = newCenterX - SheetInfo.GeneralView.ViewportHalfWidth - SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth - 0.065;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    SheetInfo.TitleBlockHeight - SheetInfo.GeneralView.ViewportHalfHeight - 0.016,
                    0);

            (SheetInfo.GeneralView.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.GeneralView.ViewportCenter = newCenter;


            return true;
        }


        // Метод для размещения основного вида пилона.
        // Позиционирование - левый верхний угол листа
        internal bool PlaceGeneralPerpendicularViewport() {

            // Проверям вдруг вид не создался
            if(SheetInfo.GeneralViewPerpendicular.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.GeneralViewPerpendicular.ViewportTypeName = "Заголовок на листе";
                SheetInfo.GeneralViewPerpendicular.ViewportNumber = "101";
                SheetInfo.GeneralViewPerpendicular.ViewportName =
                    ViewModel.ViewSectionSettings.GeneralViewPerpendicularPrefix
                    + SheetInfo.PylonKeyName
                    + ViewModel.ViewSectionSettings.GeneralViewPerpendicularSuffix;
            }


            // Передаем основной перпендикулярный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.GeneralViewPerpendicular)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона
            double newCenterX = -SheetInfo.TitleBlockWidth + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + 0.065;

            // Рассчитываем и задаем корректную точку вставки основного перпендикулярного вида пилона, если размещен основной вид
            if(SheetInfo.GeneralView.ViewportElement != null) {
                newCenterX = newCenterX - SheetInfo.GeneralView.ViewportHalfWidth - SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth - 0.065;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    SheetInfo.TitleBlockHeight - SheetInfo.GeneralViewPerpendicular.ViewportHalfHeight - 0.016,
                    0);

            (SheetInfo.GeneralViewPerpendicular.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.GeneralViewPerpendicular.ViewportCenter = newCenter;

            return true;
        }


        internal bool PlaceTransverseFirstViewPorts() {

            // Проверям вдруг вид не создался
            if(SheetInfo.TransverseViewFirst.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.TransverseViewFirst.ViewportTypeName = "Сечение_Номер вида";
                SheetInfo.TransverseViewFirst.ViewportNumber = "1";
                SheetInfo.TransverseViewFirst.ViewportName = "";
            }


            // Передаем первый поперечный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewFirst)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
            double generalViewX = 0;
            double generalViewPerpendicularX = 0;
            double newCenterX = 0;
            double newCenterY = 0;


            // Если видовой экран основного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralView.ViewportElement != null) {
                generalViewX = SheetInfo.GeneralView.ViewportCenter.X;
            }

            // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                generalViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
            }


            // Определяем координату Х первого поперечного вида пилона
            if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {

                if(generalViewX > generalViewPerpendicularX) {
                    newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
                } else {
                    newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
                }

            } else if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
                newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
            } else if(SheetInfo.GeneralView.ViewportElement is null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewFirst.ViewportHalfWidth;
            } else {
                // Когда обоих видовых экранов нет на листе
                newCenterX = SheetInfo.TitleBlockWidth / 2;
            }

            if(SheetInfo.TransverseViewSecond.ViewportElement != null || SheetInfo.TransverseViewThird.ViewportElement != null) {
                newCenterY = UnitUtilsHelper.ConvertToInternalValue(-25);
            } else {
                newCenterY = 0.016 + SheetInfo.TransverseViewFirst.ViewportHalfHeight;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    newCenterY,
                    0);


            (SheetInfo.TransverseViewFirst.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.TransverseViewFirst.ViewportCenter = newCenter;

            return true;
        }



        internal bool PlaceTransverseSecondViewPorts() {

            // Проверям вдруг вид не создался
            if(SheetInfo.TransverseViewSecond.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.TransverseViewSecond.ViewportTypeName = "Сечение_Номер вида";
                SheetInfo.TransverseViewSecond.ViewportNumber = "2";
                SheetInfo.TransverseViewSecond.ViewportName = "";
            }


            // Передаем второй поперечный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewSecond)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
            double generalViewX = 0;
            double generalViewPerpendicularX = 0;
            double newCenterX = 0;
            double newCenterY = 0;

            // Если видовой экран основного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralView.ViewportElement != null) {
                generalViewX = SheetInfo.GeneralView.ViewportCenter.X;
            }

            // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                generalViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
            }


            // Определяем координату Х первого поперечного вида пилона
            if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                if(generalViewX > generalViewPerpendicularX) {
                    newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
                } else {
                    newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
                }
            } else if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
                newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
            } else if(SheetInfo.GeneralView.ViewportElement is null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewSecond.ViewportHalfWidth;
            } else {
                // Когда обоих видовых экранов нет на листе
                newCenterX = SheetInfo.TitleBlockWidth / 2;
            }

            if(SheetInfo.TransverseViewFirst.ViewportElement is null || SheetInfo.TransverseViewThird.ViewportElement != null) {
                newCenterY = UnitUtilsHelper.ConvertToInternalValue(-50);
            } else {
                newCenterY = SheetInfo.TransverseViewFirst.ViewportCenter.Y
                    + SheetInfo.TransverseViewFirst.ViewportHalfHeight
                    + SheetInfo.TransverseViewSecond.ViewportHalfHeight;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    newCenterY,
                    0);


            (SheetInfo.TransverseViewSecond.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.TransverseViewSecond.ViewportCenter = newCenter;

            return true;
        }

        internal bool PlaceTransverseThirdViewPorts() {

            // Проверям вдруг вид не создался
            if(SheetInfo.TransverseViewThird.ViewElement == null) {
                return false;
            } else {
                // Заполнеяем данные для задания
                SheetInfo.TransverseViewThird.ViewportTypeName = "Сечение_Номер вида";
                SheetInfo.TransverseViewThird.ViewportNumber = "3";
                SheetInfo.TransverseViewThird.ViewportName = "";
            }


            // Передаем третий поперечный вид пилона в метод по созданию видов в (0.0.0)
            if(!PlacePylonViewport(SheetInfo.PylonViewSheet, SheetInfo.TransverseViewThird)) {
                return false;
            }

            // Рассчитываем и задаем корректную точку вставки первого поперечного вида пилона
            double generalViewX = 0;
            double generalViewPerpendicularX = 0;
            double newCenterX = 0;
            double newCenterY = 0;

            // Если видовой экран основного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralView.ViewportElement != null) {
                generalViewX = SheetInfo.GeneralView.ViewportCenter.X;
            }

            // Если видовой экран основного перпендикулярного вида размещен на листе, то находим его Х центра
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                generalViewPerpendicularX = SheetInfo.GeneralViewPerpendicular.ViewportCenter.X;
            }


            // Определяем координату Х первого поперечного вида пилона
            if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                if(generalViewX > generalViewPerpendicularX) {
                    newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
                } else {
                    newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
                }
            } else if(SheetInfo.GeneralView.ViewportElement != null && SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
                newCenterX = generalViewX + SheetInfo.GeneralView.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
            } else if(SheetInfo.GeneralView.ViewportElement is null && SheetInfo.GeneralViewPerpendicular.ViewportElement != null) {
                newCenterX = generalViewPerpendicularX + SheetInfo.GeneralViewPerpendicular.ViewportHalfWidth + SheetInfo.TransverseViewThird.ViewportHalfWidth;
            } else {
                // Когда обоих видовых экранов нет на листе
                newCenterX = SheetInfo.TitleBlockWidth / 2;
            }

            if(SheetInfo.TransverseViewSecond.ViewportElement is null) {
                newCenterY = UnitUtilsHelper.ConvertToInternalValue(-75);

            } else {
                newCenterY = SheetInfo.TransverseViewSecond.ViewportCenter.Y
                    + SheetInfo.TransverseViewSecond.ViewportHalfHeight
                    + SheetInfo.TransverseViewThird.ViewportHalfHeight;
            }

            XYZ newCenter = new XYZ(
                    newCenterX,
                    newCenterY,
                    0);


            (SheetInfo.TransverseViewThird.ViewportElement as Viewport).SetBoxCenter(newCenter);

            SheetInfo.TransverseViewThird.ViewportCenter = newCenter;

            return true;
        }



        internal bool PlacePylonViewport(ViewSheet viewSheet, PylonView pylonView) {

            Document doc = Repository.Document;
            // Проверяем можем ли разместить на листе видовой экран вида
            if(!Viewport.CanAddViewToSheet(doc, viewSheet.Id, pylonView.ViewElement.Id)) {
                return false;
            }


            pylonView.ViewElement.get_Parameter(BuiltInParameter.SECTION_COARSER_SCALE_PULLDOWN_METRIC).Set(100);

            // Размещаем сечение пилона на листе
            Viewport viewPort = null;
            try {
                viewPort = Viewport.Create(doc, viewSheet.Id, pylonView.ViewElement.Id, new XYZ(0, 0, 0));
            } catch(Exception) {
                return false;
            }

            pylonView.ViewportElement = viewPort;
            viewPort.LookupParameter("Номер вида").Set(pylonView.ViewportNumber);
            viewPort.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(pylonView.ViewportName);

            // Задание правильного типа видового экрана
            ICollection<ElementId> typesOfViewPort = viewPort.GetValidTypes();
            foreach(ElementId typeId in typesOfViewPort) {
                ElementType type = doc.GetElement(typeId) as ElementType;
                if(type == null) {
                    continue;
                }

                if(type.Name == pylonView.ViewportTypeName) {
                    viewPort.ChangeTypeId(type.Id);
                    break;
                }
            }

            SheetInfo.GetInfoAboutViewport(pylonView, viewPort);

            // Задание правильного положения метки видового экрана
#if REVIT_2021_OR_LESS

            //report += "Вы работаете в Revit 2020 или 2021, поэтому имя вида необходимо будет спозиционировать на листе самостоятельно.";
            //report += string.Format("Вы работаете в Revit 2020 или 2021, поэтому метку имени вида \"{0}\" необходимо будет спозиционировать на листе самостоятельно" 
            //+ Environment.NewLine, ViewElement.Name);
#else
            viewPort.LabelOffset = new XYZ(pylonView.ViewportHalfWidth, 2 * pylonView.ViewportHalfHeight - 0.022, 0);
#endif

            return true;
        }
    }
}
