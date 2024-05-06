using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPylonReinforcement.Models;

namespace RevitPylonReinforcement.ViewModels {
    class SettingsPageViewModel : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _someText;

        public SettingsPageViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            SomeText = "1";

            UIApplication uiApp = new UIApplication(_revitRepository.UIApplication.Application);
            //uiApp.Idling += new EventHandler<IdlingEventArgs>(idleUpdate);
            uiApp.Idling += new EventHandler<IdlingEventArgs>(idleUpdateForElems);


            _revitRepository.Application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        }

        public ICommand AcceptViewCommand { get; }

        public string SomeText {
            get => _someText;
            set => this.RaiseAndSetIfChanged(ref _someText, value);
        }



        public void idleUpdate(object sender, IdlingEventArgs e) {

            e.SetRaiseWithoutDelay();

            if(SomeText != DateTime.Now.ToString()) {

                SomeText = DateTime.Now.ToString();
            }
        }

        public void idleUpdateForElems(object sender, IdlingEventArgs e) {

            if(Temp.Count > 0) {

                Document doc = _revitRepository.Document;
                using(Transaction t = new Transaction(doc, "Element change")) {

                    t.Start();
                    foreach(ElementId id in Temp) {

                        Wall wall = doc.GetElement(id) as Wall;

                        if(wall is null) {

                            continue;
                        }
                        wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set("kuku");
                    }
                    t.Commit();
                }

                Temp.Clear();
            }
        }



        public static List<ElementId> Temp { get; set; } = new List<ElementId>();


        static void OnDocumentChanged(object sender, DocumentChangedEventArgs e) {

            if(e.Operation == UndoOperation.TransactionCommitted && e.GetAddedElementIds().Count > 0) {

                Temp = e.GetAddedElementIds().ToList();
            }
        }





        private void AcceptView() {

            TaskDialog.Show("Ok", "I'm okey!");
        }


        private bool CanAcceptView() {

            return true;
        }
    }
}
