using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPylonReinforcement.Models;
using RevitPylonReinforcement.Views;

namespace RevitPylonReinforcement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }


        /// <summary>
        /// Метод для команды,отрабатывающей во время загрузки окна
        /// </summary>
        private void LoadView() {

            #region
            //Floor floor = _revitRepository.ActiveUIDocument.Selection.GetElementIds().Select(id => _revitRepository.Document.GetElement(id) as Floor).FirstOrDefault();


            //Options opt = new Options();
            //opt.ComputeReferences = true;
            //GeometryElement geomElem = floor.get_Geometry(opt);

            //Face face = null;

            //foreach(GeometryObject geomObj in geomElem) {

            //    Solid solid = geomObj as Solid;

            //    if(solid is null) { continue; }

            //    foreach(Face faceTemp in solid.Faces) {

            //        if(faceTemp is null) { continue; }

            //        if(faceTemp.ComputeNormal(new UV()).Z == 1) {
            //            face = faceTemp;
            //            break;
            //        }
            //    }
            //}

            //if(face is null) {
            //    TaskDialog.Show("fd", "Ахтунг, FACE is NULL");
            //}




            //EdgeArrayArray curveArr = face.EdgeLoops;
            //List<GeometryObject> curves = new List<GeometryObject>();

            //foreach(EdgeArray edgeArray in curveArr) {
            //    foreach(Edge item in edgeArray) {
            //        curves.Add(item.AsCurve());
            //    }
            //}

            //TaskDialog.Show("fd", curves.Count.ToString());
            #endregion





            //List<Face> faces = new List<Face>();
            //FamilyInstance pylon = _revitRepository.ActiveUIDocument.Selection.GetElementIds().Select(id => _revitRepository.Document.GetElement(id) as FamilyInstance).FirstOrDefault();

            //PylonModel pylonModel = new PylonModel(pylon);

            //List<GeometryObject> curvesForPrint = new List<GeometryObject>();

            //curvesForPrint.Add(pylonModel.DrivingCurve);

            //Line newLine2 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Bottom1Pt);
            //Line newLine3 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Bottom2Pt);
            //Line newLine4 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Bottom3Pt);
            //Line newLine5 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Bottom4Pt);

            //Line newLine6 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Top1Pt);
            //Line newLine7 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Top2Pt);
            //Line newLine8 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Top3Pt);
            //Line newLine9 = Line.CreateBound(pylonModel.TopMiddlePt, pylonModel.Top4Pt);


            //curvesForPrint.Add(newLine2);
            //curvesForPrint.Add(newLine3);
            //curvesForPrint.Add(newLine4);
            //curvesForPrint.Add(newLine5);

            //curvesForPrint.Add(newLine6);
            //curvesForPrint.Add(newLine7);
            //curvesForPrint.Add(newLine8);
            //curvesForPrint.Add(newLine9);




            //using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор АР")) {

            //    DirectShape ds = DirectShape.CreateElement(_revitRepository.Document, new ElementId(BuiltInCategory.OST_GenericModel));
            //    ds.ApplicationId = "Application id";
            //    ds.ApplicationDataId = "Geometry object id";

            //    ds.SetShape(curvesForPrint);

            //    transaction.Commit();
            //}



            // Подгружаем сохраненные данные прошлого запуска
            LoadConfig();
        }



        /// <summary>
        /// Метод для команды, отрабатываеющий при нажатии "Ок"
        /// </summary>
        private void AcceptView() {


            // Метод по добавлению параметров
            SaveConfig();


            string oldDocPath = _revitRepository.Document.PathName;
            if(string.IsNullOrEmpty(oldDocPath)) {
                TaskDialog.Show("Ошибка!", "Необходимо сохранить проект перед запуском этого плагина!");
                return;
            }


            // Формирование закрепляемой панели
            DockablePaneId dockablePaneId = new DockablePaneId(new Guid("{8019815A-EFC5-4C82-9462-338670C274C1}"));


            UIApplication uIApplication = _revitRepository.UIApplication;
            DockablePane dockablePane;

            // Пытаемся получить закрепляемую панель, если еще не создана - создаем
            try {

                dockablePane = uIApplication.GetDockablePane(dockablePaneId);
                dockablePane.Show();

            } catch(Exception) {

                DockablePaneProviderData data = new DockablePaneProviderData();
                SettingsPage settingsPage = new SettingsPage();

                SettingsPageViewModel settingsPageViewModel = new SettingsPageViewModel(_pluginConfig, _revitRepository);
                settingsPage.DataContext = settingsPageViewModel;

                data.FrameworkElement = settingsPage as FrameworkElement;
                //data.InitialState = new DockablePaneState();
                ////data.InitialState.DockPosition = DockPosition.Right;
                //data.InitialState.TabBehind = DockablePanes.BuiltInDockablePanes.PropertiesPalette;

                uIApplication.RegisterDockablePane(dockablePaneId, "Корректор свойств", settingsPage as IDockablePaneProvider);

                // Формирование пути к временному файлу
                string revitVersion = _revitRepository.Application.VersionNumber;
                string pathForTempFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    + $"\\dosymep\\{revitVersion}\\RevitPylonReinforcement\\TempFile.rvt";

                // Формирование временного файла
                Document tempDocument = _revitRepository.Application.NewProjectDocument(UnitSystem.Metric);
                SaveAsOptions saveAsOptions = new SaveAsOptions();
                saveAsOptions.OverwriteExistingFile = true;
                saveAsOptions.MaximumBackups = 1;

                tempDocument.SaveAs(pathForTempFile, saveAsOptions);

                // Открытие с интерфейсом временного файла
                uIApplication.OpenAndActivateDocument(pathForTempFile);

                // Переход обратно к целевому файлу
                uIApplication.OpenAndActivateDocument(oldDocPath);

                // Закрытие и удаление временного файла
                tempDocument.Close(false);
                System.IO.File.Delete(pathForTempFile);

                dockablePane = uIApplication.GetDockablePane(dockablePaneId);
            }




            //TaskDialog.Show("fd", dockablePane.Id.ToString());





            //System.Threading.Thread.Sleep(5000);
            //tempUIDocument.SaveAndClose();
            // Не выходит закрыть документ, виды которого не активны из-за того, что есть какие то транзакции
            //tempDocument.Close(false);


            //DockablePane dp = _revitRepository.UIApplication.GetDockablePane(dpid);
            //TaskDialog.Show("fd", dp.IsShown().ToString());
            //dp.Show();





            //UIApplication uiApp = new UIApplication(_revitRepository.UIApplication.Application);
            //Document doc = _revitRepository.UIApplication.ActiveUIDocument.Document;
            //using(Transaction t = new Transaction(doc, "Text Note Creation")) {
            //    t.Start();
            //    oldDateTime = DateTime.Now.ToString();
            //    ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            //    textNote = TextNote.Create(doc, doc.ActiveView.Id, XYZ.Zero, oldDateTime, defaultTextTypeId);
            //    t.Commit();
            //}
            //uiApp.Idling += new EventHandler<IdlingEventArgs>(idleUpdate);


            //private TextNote textNote = null;
            //private String oldDateTime = null;


            //public void idleUpdate(object sender, IdlingEventArgs e) {
            //    UIApplication uiApp = sender as UIApplication;
            //    Document doc = uiApp.ActiveUIDocument.Document;
            //    if(oldDateTime != DateTime.Now.ToString()) {
            //        using(Transaction transaction = new Transaction(doc, "Text Note Update")) {
            //            transaction.Start();
            //            textNote.Text = DateTime.Now.ToString();
            //            transaction.Commit();
            //        }
            //        oldDateTime = DateTime.Now.ToString();
            //    }
        }



        public void GetCurvesFromABeam(FamilyInstance beam, Options options) {
            GeometryElement geomElem = beam.get_Geometry(options);

            CurveArray curves = new CurveArray();
            System.Collections.Generic.List<Solid> solids = new System.Collections.Generic.List<Solid>();

            //Find all solids and insert them into solid array
            AddCurvesAndSolids(geomElem, ref curves, ref solids);

            TaskDialog.Show("fd", curves.Size.ToString());
        }

        private void AddCurvesAndSolids(GeometryElement geomElem,
                                        ref CurveArray curves,
                                        ref List<Solid> solids) {
            foreach(GeometryObject geomObj in geomElem) {
                Curve curve = geomObj as Curve;
                if(null != curve) {
                    curves.Append(curve);
                    continue;
                }
                Solid solid = geomObj as Solid;
                if(null != solid) {
                    solids.Add(solid);
                    continue;
                }
                //If this GeometryObject is Instance, call AddCurvesAndSolids
                GeometryInstance geomInst = geomObj as GeometryInstance;
                if(null != geomInst) {
                    GeometryElement transformedGeomElem = geomInst.GetInstanceGeometry(geomInst.Transform);
                    AddCurvesAndSolids(transformedGeomElem, ref curves, ref solids);
                }
            }
        }









        private bool CanAcceptView() {
            //if(SelectedParams.Count > 0) {
            //    foreach(SharedParam item in SelectedParams) {
            //        if(item.SelectedParamGroupInFM is null) {
            //            ErrorText = "Назначьте группу всем выбранным параметрам";
            //            return false;
            //        }
            //    }
            //    ErrorText = string.Empty;
            //    return true;
            //} else {
            //    ErrorText = "Выберите параметры для добавления";
            //    return false;
            //}
            return true;
        }


        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            //SaveProperty = setting?.SaveProperty ?? "Привет Revit!";
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            //setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
