﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.Handlers;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitClashDetective.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly RevitEventHandler _revitEventHandler;
        private static readonly HashSet<string> _endings = new HashSet<string> { "_отсоединено", "_detached" };
        private readonly string _clashViewName = "BIM_Проверка на коллизии";
        private readonly View3D _view;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _revitEventHandler = new RevitEventHandler();
            _endings.Add("_" + _application.Username);

            _view = GetClashView();

            CommonConfig = RevitClashDetectiveConfig.GetRevitClashDetectiveConfig();

            InitializeDocInfos();
        }

        public static string ProfilePath {
            get {
                var path = @"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";
                if(!Directory.Exists(path)) {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep");
                }
                return path;
            }
        }

        public static List<BuiltInParameter> BaseLevelParameters => new List<BuiltInParameter>() {
            BuiltInParameter.MULTISTORY_STAIRS_REF_LEVEL,
            BuiltInParameter.DPART_BASE_LEVEL,
            BuiltInParameter.STAIRS_BASE_LEVEL,
            BuiltInParameter.FABRICATION_LEVEL_PARAM,
            BuiltInParameter.TRUSS_ELEMENT_REFERENCE_LEVEL_PARAM,
            BuiltInParameter.GROUP_LEVEL,
            BuiltInParameter.SPACE_REFERENCE_LEVEL_PARAM,
            BuiltInParameter.RBS_START_LEVEL_PARAM,
            BuiltInParameter.STAIRS_RAILING_BASE_LEVEL_PARAM,
            BuiltInParameter.IMPORT_BASE_LEVEL,
            BuiltInParameter.STAIRS_BASE_LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
            BuiltInParameter.FACEROOF_LEVEL_PARAM,
            BuiltInParameter.ROOF_BASE_LEVEL_PARAM,
            BuiltInParameter.ROOF_CONSTRAINT_LEVEL_PARAM,
            BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM };

        public RevitClashDetectiveConfig CommonConfig { get; set; }

        public Document Doc => _document;

        public UIApplication UiApplication => _uiApplication;

        public List<DocInfo> DocInfos { get; set; }

        public View3D GetClashView() {
            var view = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name.Equals(_clashViewName + "_" + _application.Username, StringComparison.CurrentCultureIgnoreCase));
            if(view == null) {
                using(Transaction t = _document.StartTransaction("Создание 3D-вида")) {
                    var type = new FilteredElementCollector(_document)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                    type.DefaultTemplateId = ElementId.InvalidElementId;
                    view = View3D.CreateIsometric(_document, type.Id);
                    view.Name = _clashViewName + "_" + _application.Username;
                    var categories = new[] { new ElementId(BuiltInCategory.OST_Levels),
                        new ElementId(BuiltInCategory.OST_WallRefPlanes),
                        new ElementId(BuiltInCategory.OST_Grids),
                        new ElementId(BuiltInCategory.OST_VolumeOfInterest) };

                    foreach(var category in categories) {
                        if(view.CanCategoryBeHidden(category)) {
                            view.SetCategoryHidden(category, true);
                        }
                    }

                    var bimGroup = new FilteredElementCollector(_document)
                        .OfClass(typeof(View3D))
                        .Cast<View3D>()
                        .Where(item => !item.IsTemplate)
                        .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
                        .FirstOrDefault(item => item != null && item.Contains("BIM"));
                    if(bimGroup != null) {
                        view.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, bimGroup);
                    }

                    t.Commit();
                }
            }
            return view;
        }

        public string GetDocumentName() {
            return GetDocumentName(_document);
        }

        public string GetFileDialogPath() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if(!string.IsNullOrEmpty(CommonConfig.LastRunPath) && Directory.Exists(CommonConfig.LastRunPath)) {
                path = CommonConfig.LastRunPath;
            }
            return path;
        }

        public static string GetDocumentName(Document doc) {
            var title = doc.Title;
            return GetDocumentName(title);
        }

        public static string GetDocumentName(string fileName) {
            foreach(var ending in _endings) {
                if(fileName.IndexOf(ending) > -1) {
                    fileName = fileName.Substring(0, fileName.IndexOf(ending));
                }
            }
            return fileName;
        }

        public string GetObjectName() {
            return _document.Title.Split('_').FirstOrDefault();
        }

        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public Element GetElement(string fileName, ElementId id) {
            Document doc;
            if(fileName == null) {
                doc = _document;
            } else {
                doc = DocInfos.FirstOrDefault(item => item.Name.Equals(GetDocumentName(fileName), StringComparison.CurrentCultureIgnoreCase))?.Doc;
            }
            var elementId = id;
            if(doc == null || elementId.IsNull()) {
                return null;
            }

            return doc.GetElement(elementId);
        }

        public Element GetElement(Document doc, ElementId id) {
            return doc.GetElement(id);
        }

        public List<Collector> GetCollectors() {
            return DocInfos
                .Select(item => new Collector(item.Doc))
                .ToList();
        }

        public List<WorksetCollector> GetWorksetCollectors() {
            return DocInfos
                .Select(item => new WorksetCollector(item.Doc))
                .ToList();
        }

        public View3D GetNavisworksView(Document doc) {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name == "Navisworks");
        }

        public View3D Get3DView(Document doc) {
            return GetNavisworksView(doc) ?? new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>().Where(item => !item.IsTemplate).FirstOrDefault();
        }

        public LanguageType GetLanguage() {
            return _application.Language;
        }

        public List<ParameterFilterElement> GetFilters() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .Where(item => item.Name.StartsWith("BIM"))
                .ToList();
        }

        public List<RevitLinkInstance> GetRevitLinkInstances() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(item => item.GetLinkDocument() != null)
                .ToList();
        }

        public bool IsParentLink(RevitLinkInstance link) {
            if(link.GetTypeId().IsNotNull()) {
                var type = _document.GetElement(link.GetTypeId());
                if(type is RevitLinkType linkType) {
                    return !linkType.IsNestedLink;
                } else {
                    return false;
                }
            }
            return false;
        }

        public void InitializeDocInfos() {
            DocInfos = GetRevitLinkInstances()
               .Select(item => new DocInfo(GetDocumentName(item.GetLinkDocument()), item.GetLinkDocument(), item.GetTransform()))
               .ToList();
            DocInfos.Add(new DocInfo(GetDocumentName(), _document, Transform.Identity));
        }

        public bool IsValidElement(Document doc, ElementId elementId) {
            return doc.GetElement(elementId) != null;
        }

        public void SelectElements(IEnumerable<Element> elements) {
            var selection = _uiApplication.ActiveUIDocument.Selection;
            selection.SetElementIds(elements.Select(e => e.Id).ToList());
        }

        public List<Category> GetCategories() {
            return ParameterFilterUtilities.GetAllFilterableCategories()
                .Select(item => Category.GetCategory(_document, item))
                .OfType<Category>()
                .ToList();
        }

        public Category GetCategory(BuiltInCategory builtInCategory) {
            return Category.GetCategory(_document, builtInCategory);
        }

        public List<ParameterValueProvider> GetParameters(Document doc, IEnumerable<Category> categories) {
            return categories
                .SelectMany(item => ParameterFilterUtilities.GetFilterableParametersInCommon(doc, new[] { item.Id }).Select(p => GetParam(doc, item, p)))
                .Where(item => item.RevitParam.Id != Enum.GetName(typeof(BuiltInParameter), BuiltInParameter.ELEM_PARTITION_PARAM))
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > categories.Count() - 1)
                .SelectMany(item => FilterParamProviders(item))
                .ToList();
        }

        public IEnumerable<Element> GetFilteredElements(Document doc, IEnumerable<ElementId> categories, ElementFilter filter) {
            return new FilteredElementCollector(doc)
                .WherePasses(new ElementMulticategoryFilter(categories.ToList()))
                .WhereElementIsNotElementType()
                .WherePasses(filter);
        }

        /// <summary>
        /// Выбирает элементы на 3D виде и делает подрезку
        /// </summary>
        /// <param name="elements">Элементы для выбора</param>
        /// <param name="view">3D вид, на котором надо выбрать и показать элементы</param>
        public void SelectAndShowElement(IEnumerable<ElementModel> elements, View3D view = null) {
            SelectAndShowElement(elements, 10, view);
        }

        public void SelectAndShowElement(IEnumerable<ElementModel> elements, double additionalSize, View3D view = null) {
            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }

            var bbox = GetCommonBoundingBox(elements);
            SelectAndShowElement(elements.Select(item => item.GetElement(DocInfos)), bbox, additionalSize, view);
        }

        private void SelectAndShowElement(
            IEnumerable<Element> elements,
            BoundingBoxXYZ elementsBBox,
            double additionalSize,
            View3D view = null) {

            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }
            try {
                view = view ?? _view;
                _uiDocument.ActiveView = view;

                if(elementsBBox != null) {
                    _revitEventHandler.TransactAction = () => {
                        SetSectionBox(elementsBBox, view, additionalSize);
                    };
                    _revitEventHandler.Raise();
                }
                _uiDocument.Selection.SetElementIds(GetElementsToSelect(elements, view));
            } catch(AccessViolationException) {
                ShowErrorMessage("Окно плагина было открыто в другом документе Revit, который был закрыт, нельзя показать элемент.");
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                ShowErrorMessage("Окно плагина было открыто в другом документе Revit, который сейчас не активен, нельзя показать элемент.");
            }
        }

        public void DoAction(Action action) {
            _revitEventHandler.TransactAction = action;
            _revitEventHandler.Raise();
        }

        public static string GetLevelName(Element element) {
            return GetLevel(element)?.Name;
        }

        public static Level GetLevel(Element element) {
            ElementId levelId;
            foreach(var paramName in BaseLevelParameters) {
                levelId = element.GetParamValueOrDefault<ElementId>(paramName);
                if(levelId.IsNotNull()) {
                    return element.Document.GetElement(levelId) as Level;
                }
            }
            if(element.LevelId.IsNotNull()) {
                return element.Document.GetElement(element.LevelId) as Level;
            }
            return null;
        }


        private Transform GetDocumentTransform(string docTitle) {
            if(docTitle.Equals(GetDocumentName(), StringComparison.CurrentCultureIgnoreCase))
                return Transform.Identity;
            return GetRevitLinkInstances()
                .FirstOrDefault(item => GetDocumentName(item.GetLinkDocument())
                                        .Equals(docTitle, StringComparison.CurrentCultureIgnoreCase))
                ?.GetTotalTransform();
        }

        private ICollection<ElementId> GetElementsToSelect(IEnumerable<Element> elements, View3D view = null) {
            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }

            List<ElementId> elementsFromThisDoc = elements
                .Where(item => item.IsFromDocument(_document))
                .Select(item => item.Id)
                .ToList();
            if(elementsFromThisDoc.Count > 0) {
                return elementsFromThisDoc;
            } else {
                var view3d = view ?? _view;
                return view3d.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_SectionBox));
            }
        }

        private void ShowErrorMessage(string message) {
            var dialog = GetPlatformService<IMessageBoxService>();
            dialog.Show(
                message,
                $"BIM",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }

        private BoundingBoxXYZ GetCommonBoundingBox(IEnumerable<ElementModel> elements) {
            return elements
                .Select(item => new {
                    Bb = item.GetElement(DocInfos).get_BoundingBox(null),
                    Transform = item.GetDocInfo(DocInfos).Transform
                })
                .Where(item => item.Bb != null)
                .Select(item => item.Bb.GetTransformedBoundingBox(item.Transform))
                .GetCommonBoundingBox();
        }

        private ParameterValueProvider GetParam(Document doc, Category category, ElementId elementId) {
            var revitParam = ParameterInitializer.InitializeParameter(doc, elementId);
            if(elementId.IsSystemId()) {
                return new ParameterValueProvider(this, revitParam, $"{revitParam.Name} ({category.Name})");
            }
            return new ParameterValueProvider(this, revitParam);
        }

        private IEnumerable<ParameterValueProvider> FilterParamProviders(IEnumerable<ParameterValueProvider> providers) {
            if(providers.First().RevitParam is SystemParam) {
                if(providers.All(item => item.RevitParam.Id.Equals(providers.First().RevitParam.Id, StringComparison.CurrentCultureIgnoreCase))) {
                    providers.First().DisplayValue = providers.First().Name;
                    return new[] { providers.First() };
                }
                return providers;
            }
            return new[] { providers.First() };
        }

        /// <summary>
        /// Устанавливает область подрезки, увеличивая размеры ограничивающего бокса на заданное количество футов
        /// <para>Размеры области подрезки будут формироваться как сумма габарита бокса по соответствующей оси и добавочного размера</para>
        /// </summary>
        /// <param name="bb">Ограничивающий бокс, на основе которого будет строиться область подрезки</param>
        /// <param name="view">3D Вид для задания области подрезки</param>
        /// <param name="additionalSize">Добавочный размер в футах, на который нужно увеличить бокс в каждом направлении: OX, OY, OZ</param>
        private void SetSectionBox(BoundingBoxXYZ bb, View3D view, double additionalSize) {
            if(bb == null)
                return;
            using(Transaction t = _document.StartTransaction("Подрезка")) {
                double halfSize = Math.Abs(additionalSize / 2);
                bb.Max += new XYZ(halfSize, halfSize, halfSize);
                bb.Min -= new XYZ(halfSize, halfSize, halfSize);
                view.SetSectionBox(bb);
                var uiView = _uiDocument.GetOpenUIViews().FirstOrDefault(item => item.ViewId == view.Id);
                if(uiView != null) {
                    uiView.ZoomAndCenterRectangle(bb.Min, bb.Max);
                }
                t.Commit();
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}