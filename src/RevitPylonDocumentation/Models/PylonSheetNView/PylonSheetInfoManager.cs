using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    internal class PylonSheetInfoManager {


        public PylonSheetInfoManager(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }


        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }

        public void WorkWithCreation() {

            UserSelectionSettings selectionSettings = ViewModel.SelectionSettings;

            // Если текущий PylonSheetInfo не выбран для работы - continue
            if(!SheetInfo.IsCheck) { return; } else {
                SheetInfo.GetViewNamesForWork();
            }

            // Если листы был в проекте (когда плагин запускают для создания/размещения видов), то мы об этом знаем из RevitRepository
            if(SheetInfo.PylonViewSheet is null) {

                SheetInfo.CreateSheet();
            } else {

                SheetInfo.FindTitleBlock();
                SheetInfo.GetTitleBlockSize();
                SheetInfo.FindViewsNViewportsOnSheet();
                SheetInfo.FindSchedulesNViewportsOnSheet();
                SheetInfo.FindNoteLegendOnSheet();
            }

            // Если вдруг по какой-то причине лист не был создан, то создание видов/видовых экранов не выполняем 
            if(SheetInfo.PylonViewSheet is null) { return; }

            //////////////////
            // ОСНОВНОЙ ВИД //
            //////////////////

            if(selectionSettings.NeedWorkWithGeneralView) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.GeneralView.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.GeneralView.ViewSectionCreator.TryCreateGeneralView(ViewModel.SelectedViewFamilyType)) {
                        Repository.FindViewSectionInPj(SheetInfo.GeneralView);
                    }
                }
                // Тут точно получили вид
            }


            ///////////////////////////////////
            // ОСНОВНОЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД //
            ///////////////////////////////////

            if(selectionSettings.NeedWorkWithGeneralPerpendicularView) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.GeneralViewPerpendicular.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.GeneralViewPerpendicular.ViewSectionCreator.TryCreateGeneralPerpendicularView(ViewModel.SelectedViewFamilyType)) {
                        Repository.FindViewSectionInPj(SheetInfo.GeneralViewPerpendicular);
                    }
                }
                // Тут точно получили вид
            }


            ///////////////////////////
            // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД //
            ///////////////////////////

            if(selectionSettings.NeedWorkWithTransverseViewFirst) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.TransverseViewFirst.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.TransverseViewFirst.ViewSectionCreator.TryCreateTransverseView(ViewModel.SelectedViewFamilyType, 1)) {
                        Repository.FindViewSectionInPj(SheetInfo.TransverseViewFirst);
                    }
                }
                // Тут точно получили вид
            }

            ///////////////////////////
            // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД //
            ///////////////////////////

            if(selectionSettings.NeedWorkWithTransverseViewSecond) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.TransverseViewSecond.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.TransverseViewSecond.ViewSectionCreator.TryCreateTransverseView(ViewModel.SelectedViewFamilyType, 2)) {
                        Repository.FindViewSectionInPj(SheetInfo.TransverseViewSecond);
                    }
                }
                // Тут точно получили вид
            }

            ///////////////////////////
            // ТРЕТИЙ ПОПЕРЕЧНЫЙ ВИД //
            ///////////////////////////

            if(selectionSettings.NeedWorkWithTransverseViewThird) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.TransverseViewThird.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.TransverseViewThird.ViewSectionCreator.TryCreateTransverseView(ViewModel.SelectedViewFamilyType, 3)) {
                        Repository.FindViewSectionInPj(SheetInfo.TransverseViewThird);
                    }
                }
                // Тут точно получили вид
            }

            ///////////////////////////
            // СПЕЦИФИКАЦИЯ АРМАТУРЫ //
            ///////////////////////////

            if(selectionSettings.NeedWorkWithRebarSchedule) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.RebarSchedule.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.RebarSchedule.ViewScheduleCreator.TryCreateRebarSchedule()) {
                        Repository.FindViewScheduleInPj(SheetInfo.RebarSchedule);
                    }
                }
                // Тут точно получили вид
            }

            /////////////////////////////
            // СПЕЦИФИКАЦИЯ МАТЕРИАЛОВ //
            /////////////////////////////

            if(selectionSettings.NeedWorkWithMaterialSchedule) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.MaterialSchedule.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.MaterialSchedule.ViewScheduleCreator.TryCreateMaterialSchedule()) {
                        Repository.FindViewScheduleInPj(SheetInfo.MaterialSchedule);
                    }
                }
                // Тут точно получили вид
            }

            /////////////////////////////////
            // ВЕДОМОСТЬ СИСТЕМНЫХ ДЕТАЛЕЙ //
            /////////////////////////////////

            if(selectionSettings.NeedWorkWithSystemPartsSchedule) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.SystemPartsSchedule.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.SystemPartsSchedule.ViewScheduleCreator.TryCreateSystemPartsSchedule()) {
                        Repository.FindViewScheduleInPj(SheetInfo.SystemPartsSchedule);
                    }
                }
                // Тут точно получили вид
            }

            ///////////////////////////
            // ВЕДОМОСТЬ IFC ДЕТАЛЕЙ //
            ///////////////////////////

            if(selectionSettings.NeedWorkWithIFCPartsSchedule) {

                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                if(SheetInfo.IFCPartsSchedule.ViewElement is null) {

                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.IFCPartsSchedule.ViewScheduleCreator.TryCreateIFCPartsSchedule()) {
                        Repository.FindViewScheduleInPj(SheetInfo.IFCPartsSchedule);
                    }
                }
                // Тут точно получили вид
            }


            // Принудительно регеним документ, иначе запрашиваемые габариты видовых экранов будут некорректны
            Repository.Document.Regenerate();


            if(selectionSettings.NeedWorkWithGeneralView) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.GeneralView.ViewportElement is null) {

                    SheetInfo.GeneralView.ViewSectionPlacer.PlaceGeneralViewport();
                }
            }
            if(selectionSettings.NeedWorkWithGeneralPerpendicularView) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {

                    SheetInfo.GeneralViewPerpendicular.ViewSectionPlacer.PlaceGeneralPerpendicularViewport();
                }
            }
            if(selectionSettings.NeedWorkWithTransverseViewFirst) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.TransverseViewFirst.ViewportElement is null) {

                    SheetInfo.TransverseViewFirst.ViewSectionPlacer.PlaceTransverseFirstViewPorts();
                }
            }
            if(selectionSettings.NeedWorkWithTransverseViewSecond) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.TransverseViewSecond.ViewportElement is null) {

                    SheetInfo.TransverseViewSecond.ViewSectionPlacer.PlaceTransverseSecondViewPorts();
                }
            }
            if(selectionSettings.NeedWorkWithTransverseViewThird) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.TransverseViewThird.ViewportElement is null) {

                    SheetInfo.TransverseViewThird.ViewSectionPlacer.PlaceTransverseThirdViewPorts();
                }
            }
            if(selectionSettings.NeedWorkWithRebarSchedule) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.RebarSchedule.ViewportElement is null) {

                    SheetInfo.RebarSchedule.ViewSchedulePlacer.PlaceRebarSchedule();
                }
            }
            if(selectionSettings.NeedWorkWithMaterialSchedule) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.MaterialSchedule.ViewportElement is null) {

                    SheetInfo.MaterialSchedule.ViewSchedulePlacer.PlaceMaterialSchedule();
                }
            }
            if(selectionSettings.NeedWorkWithSystemPartsSchedule) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.SystemPartsSchedule.ViewportElement is null) {

                    SheetInfo.SystemPartsSchedule.ViewSchedulePlacer.PlaceSystemPartsSchedule();
                }
            }
            if(selectionSettings.NeedWorkWithIFCPartsSchedule) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.IFCPartsSchedule.ViewportElement is null) {

                    SheetInfo.IFCPartsSchedule.ViewSchedulePlacer.PlaceIFCPartsSchedule();
                }
            }
            if(selectionSettings.NeedWorkWithLegend) {

                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.LegendView.ViewportElement is null) {

                    SheetInfo.LegendView.LegendPlacer.PlaceNoteLegend();
                }
            }
        }

    }
}
