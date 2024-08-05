using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

using Document = Autodesk.Revit.DB.Document;
using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    internal class PylonSheetInfo : BaseViewModel {

        private bool _isCheck = false;

        internal PylonSheetInfo(MainViewModel mvm, RevitRepository repository, string pylonKeyName) {
            ViewModel = mvm;
            Repository = repository;
            PylonKeyName = pylonKeyName;

            Manager = new PylonSheetInfoManager(ViewModel, Repository, this);

            GeneralView = new PylonView(ViewModel, Repository, this);
            GeneralViewPerpendicular = new PylonView(ViewModel, Repository, this);
            TransverseViewFirst = new PylonView(ViewModel, Repository, this);
            TransverseViewSecond = new PylonView(ViewModel, Repository, this);
            TransverseViewThird = new PylonView(ViewModel, Repository, this);

            RebarSchedule = new PylonView(ViewModel, Repository, this);
            MaterialSchedule = new PylonView(ViewModel, Repository, this);
            SystemPartsSchedule = new PylonView(ViewModel, Repository, this);
            IFCPartsSchedule = new PylonView(ViewModel, Repository, this);

            LegendView = new PylonView(ViewModel, Repository, this);
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfoManager Manager { get; set; }


        public bool IsCheck {
            get => _isCheck;
            set => this.RaiseAndSetIfChanged(ref _isCheck, value);

        }

        public bool SheetInProject { get; set; } = false;

        // Марка пилона 
        public string PylonKeyName { get; set; }
        public string ProjectSection { get; set; }

        public List<Element> HostElems { get; set; } = new List<Element>();
        public ViewSheet PylonViewSheet { get; set; }


        // Рамка листа
        public FamilyInstance TitleBlock { get; set; }
        public double TitleBlockHeight { get; set; }
        public double TitleBlockWidth { get; set; }


        // Видовые экраны разразов
        public PylonView GeneralView { get; set; }
        public PylonView GeneralViewPerpendicular { get; set; }
        public PylonView TransverseViewFirst { get; set; }
        public PylonView TransverseViewSecond { get; set; }
        public PylonView TransverseViewThird { get; set; }



        // Видовые экраны спецификаций
        public PylonView RebarSchedule { get; set; }
        public PylonView MaterialSchedule { get; set; }
        public PylonView SystemPartsSchedule { get; set; }
        public PylonView IFCPartsSchedule { get; set; }


        // Легенда примечаний
        public PylonView LegendView { get; set; }



        /// <summary>
        /// Создание листа, задание имени, поиск рамки и задание ей нужных габаритов
        /// </summary>
        public bool CreateSheet() {
            if(PylonViewSheet != null) {
                return false;
            }

            PylonViewSheet = ViewSheet.Create(Repository.Document, ViewModel.SelectedTitleBlock.Id);
            PylonViewSheet.Name = ViewModel.ProjectSettings.SheetPrefix + PylonKeyName + ViewModel.ProjectSettings.SheetSuffix;

            Parameter viewSheetGroupingParameter = PylonViewSheet.LookupParameter(ViewModel.ProjectSettings.DispatcherGroupingFirst);
            if(viewSheetGroupingParameter == null) {
            } else {
                viewSheetGroupingParameter.Set(ViewModel.SelectedProjectSection);
            }

            FindTitleBlock();
            SetTitleBlockSize(Repository.Document);

            return true;
        }

        /// <summary>
        /// Поиск рамки на листе
        /// </summary>
        public bool FindTitleBlock() {

            // Ищем рамку листа
            FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, PylonViewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock is null) { return false; }
            TitleBlock = titleBlock;

            return true;
        }



        /// <summary>
        /// Получает габариты рамки листа и записывает в параметры TitleBlockWidth и TitleBlockHeight
        /// </summary>
        internal void GetTitleBlockSize() {
            if(TitleBlock is null) {
                return;
            }

            // Получение габаритов рамки листа
            BoundingBoxXYZ boundingBoxXYZ = TitleBlock.get_BoundingBox(PylonViewSheet);
            TitleBlockWidth = -1 * boundingBoxXYZ.Min.X;
            TitleBlockHeight = boundingBoxXYZ.Max.Y;
        }


        /// <summary>
        /// Задает размеры рамки листа (по дефолту ставит А3) и запоминает
        /// </summary>
        internal void SetTitleBlockSize(Document doc, int sheetSize = 3, int sheetCoefficient = 1) {
            if(TitleBlock is null) {
                return;
            }
            // Пытаемся задать габарит листа
            Parameter paramA = TitleBlock.LookupParameter(ViewModel.ProjectSettings.SheetSize);
            Parameter paramX = TitleBlock.LookupParameter(ViewModel.ProjectSettings.SheetCoefficient);


            if(paramA != null && paramX != null) {
                paramA.Set(sheetSize);
                paramX.Set(sheetCoefficient);
                doc.Regenerate();

                string format = string.Empty;
                if(sheetCoefficient > 1) {
                    format = string.Format("А{0}х{1}", sheetSize, sheetCoefficient);
                } else {
                    format = "А" + sheetSize;
                }
            }

            // Получение итоговых габаритов рамки листа
            GetTitleBlockSize();
        }


        /// <summary>
        /// Получает и сохраняет имена видов в соответствии с префиксами/суффиксами, которые указал пользователь
        /// </summary>
        public void GetViewNamesForWork() {

            GeneralView.ViewName = ViewModel.ViewSectionSettings.GeneralViewPrefix + PylonKeyName + ViewModel.ViewSectionSettings.GeneralViewSuffix;
            GeneralViewPerpendicular.ViewName = ViewModel.ViewSectionSettings.GeneralViewPerpendicularPrefix + PylonKeyName + ViewModel.ViewSectionSettings.GeneralViewPerpendicularSuffix;
            TransverseViewFirst.ViewName = ViewModel.ViewSectionSettings.TransverseViewFirstPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseViewFirstSuffix;
            TransverseViewSecond.ViewName = ViewModel.ViewSectionSettings.TransverseViewSecondPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseViewSecondSuffix;
            TransverseViewThird.ViewName = ViewModel.ViewSectionSettings.TransverseViewThirdPrefix + PylonKeyName + ViewModel.ViewSectionSettings.TransverseViewThirdSuffix;

            RebarSchedule.ViewName = ViewModel.SchedulesSettings.RebarSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.RebarScheduleSuffix;
            MaterialSchedule.ViewName = ViewModel.SchedulesSettings.MaterialSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.MaterialScheduleSuffix;
            SystemPartsSchedule.ViewName = ViewModel.SchedulesSettings.SystemPartsSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.SystemPartsScheduleSuffix;
            IFCPartsSchedule.ViewName = ViewModel.SchedulesSettings.IFCPartsSchedulePrefix + PylonKeyName + ViewModel.SchedulesSettings.IFCPartsScheduleSuffix;
        }


        /// <summary>
        /// Ищет и запоминает виды и видовые экраны через видовые экраны, размещенные на листе
        /// </summary>
        public void FindViewsNViewportsOnSheet() {

            foreach(ElementId id in PylonViewSheet.GetAllViewports()) {

                Viewport viewport = Repository.Document.GetElement(id) as Viewport;
                ViewSection viewSection = Repository.Document.GetElement(viewport.ViewId) as ViewSection;

                // Видовые экраны не разрезов нас не интересуют
                if(viewSection is null) { continue; }

                // Если вид не находили до этого в этом цикле и его имя равно нужному, то сохраняем
                // GeneralView
                if(GeneralView.ViewElement is null && viewSection.Name.Equals(GeneralView.ViewName)) {
                    GeneralView.ViewElement = viewSection;
                    GeneralView.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(GeneralView, viewport);
                    continue;
                }

                // GeneralViewPerpendicular
                if(GeneralViewPerpendicular.ViewElement is null && viewSection.Name.Equals(GeneralViewPerpendicular.ViewName)) {
                    GeneralViewPerpendicular.ViewElement = viewSection;
                    GeneralViewPerpendicular.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(GeneralViewPerpendicular, viewport);
                    continue;
                }

                // TransverseViewFirst
                if(TransverseViewFirst.ViewElement is null && viewSection.Name.Equals(TransverseViewFirst.ViewName)) {
                    TransverseViewFirst.ViewElement = viewSection;
                    TransverseViewFirst.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(TransverseViewFirst, viewport);
                    continue;
                }

                // TransverseViewSecond
                if(TransverseViewSecond.ViewElement is null && viewSection.Name.Equals(TransverseViewSecond.ViewName)) {
                    TransverseViewSecond.ViewElement = viewSection;
                    TransverseViewSecond.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(TransverseViewSecond, viewport);
                    continue;
                }

                // TransverseViewThird
                if(TransverseViewThird.ViewElement is null && viewSection.Name.Equals(TransverseViewThird.ViewName)) {
                    TransverseViewThird.ViewElement = viewSection;
                    TransverseViewThird.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutViewport(TransverseViewThird, viewport);
                    continue;
                }
            }
        }


        /// <summary>
        /// Ищет и запоминает легенду, размещенную на листе
        /// </summary>
        public void FindNoteLegendOnSheet() {

            foreach(ElementId id in PylonViewSheet.GetAllViewports()) {

                Viewport viewportLegend = Repository.Document.GetElement(id) as Viewport;
                View viewLegend = Repository.Document.GetElement(viewportLegend.ViewId) as View;

                if(viewLegend is null) { continue; }

                if(LegendView.ViewElement is null && viewLegend.Name.Equals(ViewModel.SelectedLegend.Name)) {
                    LegendView.ViewElement = viewLegend;
                    LegendView.ViewportElement = viewportLegend;

                    return;
                }
            }
        }


        /// <summary>
        /// Ищет и запоминает спеки и их видовые экраны через видовые экраны, размещенные на листе
        /// </summary>
        public void FindSchedulesNViewportsOnSheet() {

            foreach(ElementId id in PylonViewSheet.GetDependentElements(new ElementClassFilter(typeof(ScheduleSheetInstance)))) {

                ScheduleSheetInstance viewport = Repository.Document.GetElement(id) as ScheduleSheetInstance;
                ViewSchedule viewSchedule = Repository.Document.GetElement(viewport.ScheduleId) as ViewSchedule;

                if(viewSchedule is null) { continue; }

                // Если вид не находили до этого в этом цикле и его имя равно нужному, то сохраняем
                // RebarSchedule
                if(RebarSchedule.ViewElement is null && viewSchedule.Name.Equals(RebarSchedule.ViewName)) {
                    RebarSchedule.ViewElement = viewSchedule;
                    RebarSchedule.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutScheduleSheetInstance(RebarSchedule, viewport);
                    continue;
                }

                // MaterialSchedule
                if(MaterialSchedule.ViewElement is null && viewSchedule.Name.Equals(MaterialSchedule.ViewName)) {
                    MaterialSchedule.ViewElement = viewSchedule;
                    MaterialSchedule.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutScheduleSheetInstance(MaterialSchedule, viewport);
                    continue;
                }

                // SystemPartsSchedule
                if(SystemPartsSchedule.ViewElement is null && viewSchedule.Name.Equals(SystemPartsSchedule.ViewName)) {
                    SystemPartsSchedule.ViewElement = viewSchedule;
                    SystemPartsSchedule.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutScheduleSheetInstance(SystemPartsSchedule, viewport);
                    continue;
                }

                // IFCPartsSchedule
                if(IFCPartsSchedule.ViewElement is null && viewSchedule.Name.Equals(IFCPartsSchedule.ViewName)) {
                    IFCPartsSchedule.ViewElement = viewSchedule;
                    IFCPartsSchedule.ViewportElement = viewport;

                    // Получение центра и габаритов видового экрана
                    GetInfoAboutScheduleSheetInstance(IFCPartsSchedule, viewport);
                    continue;
                }
            }
        }


        /// <summary>
        /// Получение и сохранение информации о центре и габаритах видового экрана
        /// </summary>
        public void GetInfoAboutViewport(PylonView pylonView, Viewport viewport) {

            XYZ viewportCenter = viewport.GetBoxCenter();
            Outline viewportOutline = viewport.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            pylonView.ViewportCenter = viewportCenter;
            pylonView.ViewportHalfWidth = viewportHalfWidth;
            pylonView.ViewportHalfHeight = viewportHalfHeight;
        }



        /// <summary>
        /// Получение и сохранение информации о центре и габаритах видового экрана спек
        /// </summary>
        public void GetInfoAboutScheduleSheetInstance(PylonView pylonView, ScheduleSheetInstance scheduleSheetInstance) {

            // Получение габаритов видового экрана спецификации
            XYZ viewportCenter = scheduleSheetInstance.Point;
            BoundingBoxXYZ boundingBoxXYZ = scheduleSheetInstance.get_BoundingBox(pylonView.SheetInfo.PylonViewSheet);
            double scheduleHalfWidth = boundingBoxXYZ.Max.X / 2;
            double scheduleHalfHeight = -boundingBoxXYZ.Min.Y / 2;     // Создается так, что верхний левый угол спеки в нижнем правом углу рамки

            pylonView.ViewportCenter = viewportCenter;
            pylonView.ViewportHalfWidth = scheduleHalfWidth;
            pylonView.ViewportHalfHeight = scheduleHalfHeight;
        }



    }
}
