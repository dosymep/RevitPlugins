using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPylonReinforcement.Models;

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


            Floor floor = _revitRepository.ActiveUIDocument.Selection.GetElementIds().Select(id => _revitRepository.Document.GetElement(id) as Floor).FirstOrDefault();


            Options opt = new Options();
            opt.ComputeReferences = true;
            GeometryElement geomElem = floor.get_Geometry(opt);

            Face face = null;

            foreach(GeometryObject geomObj in geomElem) {

                Solid solid = geomObj as Solid;

                if(solid is null) { continue; }

                foreach(Face faceTemp in solid.Faces) {

                    if(faceTemp is null) { continue; }

                    if(faceTemp.ComputeNormal(new UV()).Z == 1) {
                        face = faceTemp;
                        break;
                    }
                }
            }

            if(face is null) {
                TaskDialog.Show("fd", "Ахтунг, FACE is NULL");
            }




            EdgeArrayArray curveArr = face.EdgeLoops;
            List<GeometryObject> curves = new List<GeometryObject>();

            foreach(EdgeArray edgeArray in curveArr) {
                foreach(Edge item in edgeArray) {
                    curves.Add(item.AsCurve());
                }
            }

            TaskDialog.Show("fd", curves.Count.ToString());



            //List<Face> faces = new List<Face>();
            //FamilyInstance pylon = _revitRepository.ActiveUIDocument.Selection.GetElementIds().Select(id => _revitRepository.Document.GetElement(id) as FamilyInstance).FirstOrDefault();

            //Options opt = new Options();
            //opt.ComputeReferences = true;
            //GeometryElement geomElem = pylon.get_Geometry(opt);

            //foreach(GeometryObject geomObj in geomElem) {

            //    Solid solid = geomObj as Solid;
            //    if(solid is null) {
            //        continue;
            //    }

            //    foreach(Face face in solid.Faces) {
            //        faces.Add(face);
            //    }
            //}

            //TaskDialog.Show("Итого faces", faces.Count.ToString());

            //SweptProfile sweptProfile = pylon.GetSweptProfile();
            //Curve curve = sweptProfile.GetDrivingCurve();
            //XYZ p0 = curve.GetEndPoint(0);
            //XYZ p1 = curve.GetEndPoint(1);

            //TaskDialog.Show("Итого faces", (p0.Z * 304.8).ToString());
            //TaskDialog.Show("Итого faces", (p1.Z * 304.8).ToString());



            //List<GeometryObject> curves = new List<GeometryObject>();
            //curves.Add(curve);

            //foreach(Curve c in sweptProfile.GetSweptProfile().Curves) {
            //    curves.Add(c);
            //}


            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор АР")) {

                DirectShape ds = DirectShape.CreateElement(_revitRepository.Document, new ElementId(BuiltInCategory.OST_GenericModel));
                ds.ApplicationId = "Application id";
                ds.ApplicationDataId = "Geometry object id";

                ds.SetShape(curves);

                transaction.Commit();
            }



            // Подгружаем сохраненные данные прошлого запуска
            LoadConfig();
        }


        /// <summary>
        /// Метод для команды, отрабатываеющий при нажатии "Ок"
        /// </summary>
        private void AcceptView() {


            // Метод по добавлению параметров
            SaveConfig();
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
