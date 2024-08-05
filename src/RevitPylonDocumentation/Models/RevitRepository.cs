using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

using Document = Autodesk.Revit.DB.Document;
using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        /// <summary>
        /// Возвращает список всех листов, имеющихся  в проекте
        /// </summary>
        public List<ViewSheet> AllSheets => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToList();

        /// <summary>
        /// Возвращает список всех сечений, имеющихся в проекте
        /// </summary>
        public List<ViewSection> AllSectionViews => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .OfType<ViewSection>()
                .ToList();

        /// <summary>
        /// Возвращает список всех спецификаций, имеющихся в проекте
        /// </summary>
        public List<ViewSchedule> AllScheduleViews => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSchedule))
                .WhereElementIsNotElementType()
                .OfType<ViewSchedule>()
                .Where(view => !view.IsTemplate)
                .ToList();

        /// <summary>
        /// Возвращает список всех типоразмеров рамок листа
        /// </summary>
        public List<FamilySymbol> TitleBlocksInProject => new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .OfType<FamilySymbol>()
                .ToList();

        /// <summary>
        /// Возвращает список типоразмеров видов в проекте
        /// </summary>
        public List<ViewFamilyType> ViewFamilyTypes => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewFamilyType))
                .OfType<ViewFamilyType>()
                .Where(a => ViewFamily.Section == a.ViewFamily)
                .ToList();
        /// <summary>
        /// Возвращает список всех легенд, присутствующих в проекте
        /// </summary>
        public List<View> LegendsInProject => new FilteredElementCollector(Document)
                .OfClass(typeof(View))
                .OfType<View>()
                .Where(view => view.ViewType == ViewType.Legend)
                .ToList();
        /// <summary>
        /// Возвращает список всех шаблонов сечений в проекте
        /// </summary>
        public List<ViewSection> AllViewTemplates => new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .OfType<ViewSection>()
                .Where(v => v.IsTemplate == true)
                .OrderBy(a => a.Name)
                .ToList();


        /// <summary>
        /// Хранит оболочки над листами пилонов - центральное хранилище информации по пилону для работы плагина
        /// </summary>
        public List<PylonSheetInfo> HostsInfo { get; set; } = new List<PylonSheetInfo>();

        public List<string> HostProjectSections { get; set; } = new List<string>();

        /// <summary>
        /// Получает информацию о всех пилонах, размещенных в проекте
        /// </summary>
        public void GetHostData(MainViewModel mainViewModel) {

            HostsInfo.Clear();
            HostProjectSections.Clear();

            foreach(var cat in new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns }) {

                IList<Element> elems = new FilteredElementCollector(Document)
                    .OfCategory(cat)
                    .WhereElementIsNotElementType()
                    .ToElements();

                AnalizePylons(mainViewModel, elems);
            }

            HostsInfo = new List<PylonSheetInfo>(HostsInfo
                .OrderBy(i => i.PylonKeyName));

            // Получаем список разделов в проекте (комплектов документации)
            HostProjectSections = new List<string>(HostsInfo
                .Select(item => item.ProjectSection)
                .Distinct()
                .OrderBy(i => i));
        }

        /// <summary>
        /// Получает информацию толко о выбранном пилоне
        /// </summary>
        public void GetHostData(MainViewModel mainViewModel, IList<Element> elems) {

            HostsInfo.Clear();
            HostProjectSections.Clear();

            AnalizePylons(mainViewModel, elems);

            // Получаем список разделов в проекте (комплектов документации)
            HostProjectSections = new List<string>(HostsInfo
                .Select(item => item.ProjectSection)
                .Distinct()
                .OrderBy(i => i));
        }

        /// <summary>
        /// Анализирует найденные пилоны, извлекая информацию о них и заполняя список оболочек над листами пилонов
        /// </summary>
        private void AnalizePylons(MainViewModel mainViewModel, IList<Element> elems) {

            foreach(Element elem in elems) {
                if(!elem.Name.Contains("Пилон")) { continue; }


                // Запрашиваем параметр фильтрации типовых пилонов. Если он не равен заданному, то отсеиваем этот пилон
                Parameter typicalPylonParameter = elem.LookupParameter(mainViewModel.ProjectSettings.TypicalPylonFilterParameter);
                if(typicalPylonParameter == null) {
                    mainViewModel.ErrorText = "Параметр фильтрации типовых пилонов не найден";
                    return;
                }

                if(typicalPylonParameter.AsString() is null || typicalPylonParameter.AsString() != mainViewModel.ProjectSettings.TypicalPylonFilterValue) { continue; }


                // Запрашиваем Раздел проекта
                Parameter projectSectionParameter = elem.LookupParameter(mainViewModel.ProjectSettings.ProjectSection);
                if(projectSectionParameter == null) {
                    mainViewModel.ErrorText = "Параметр раздела не найден у элементов Стен или Несущих колонн";
                    return;
                }
                string projectSection = projectSectionParameter.AsString();
                if(projectSection is null) { continue; }


                // Запрашиваем Марку пилона
                Parameter hostMarkParameter = elem.LookupParameter(mainViewModel.ProjectSettings.Mark);
                if(hostMarkParameter == null) {
                    mainViewModel.ErrorText = "Параметр марки не найден у элементов Стен или Несущих колонн";
                    return;
                }
                string hostMark = hostMarkParameter.AsString();
                if(hostMark is null) { continue; }


                PylonSheetInfo testPylonSheetInfo = HostsInfo
                    .Where(item => item.PylonKeyName.Equals(hostMark))
                    .FirstOrDefault();

                if(testPylonSheetInfo is null) {
                    PylonSheetInfo pylonSheetInfo = new PylonSheetInfo(mainViewModel, this, hostMark);
                    pylonSheetInfo.ProjectSection = projectSection;
                    pylonSheetInfo.HostElems.Add(elem);
                    FindSheetInPj(mainViewModel, pylonSheetInfo);

                    HostsInfo.Add(pylonSheetInfo);
                } else {
                    testPylonSheetInfo.HostElems.Add(elem);
                    mainViewModel.ErrorText = "Найдены пилоны с одинаковой маркой";
                }
            }

        }



        /// <summary>
        /// Ищет лист в проекте по информации из оболочки листа пилона PylonSheetInfo
        /// </summary>
        public void FindSheetInPj(MainViewModel mainViewModel, PylonSheetInfo pylonSheetInfo) {

            ViewSheet sheet = AllSheets
                .Where(item => item.Name.Equals(mainViewModel.ProjectSettings.SheetPrefix + pylonSheetInfo.PylonKeyName + mainViewModel.ProjectSettings.SheetSuffix))
                .FirstOrDefault();

            if(sheet != null) {
                pylonSheetInfo.SheetInProject = true;
                pylonSheetInfo.PylonViewSheet = sheet;
            }
        }

        /// <summary>
        /// Ищет в проекте вид по имени, указанному в оболочке вида пилона PylonView
        /// </summary>
        public void FindViewSectionInPj(PylonView pylonView) {

            foreach(ViewSection view in AllSectionViews) {

                if(view.Name == pylonView.ViewName) {
                    pylonView.ViewElement = view;
                    break;
                }
            }
        }

        /// <summary>
        /// Ищет в проекте спеки по имени, указанному в оболочке вида пилона PylonView
        /// </summary>
        public void FindViewScheduleInPj(PylonView pylonView) {

            foreach(ViewSchedule view in AllScheduleViews) {

                if(view.Name == pylonView.ViewName) {
                    pylonView.ViewElement = view;

                    break;
                }
            }
        }
    }
}
