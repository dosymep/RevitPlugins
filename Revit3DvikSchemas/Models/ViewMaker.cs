using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.Revit;

using Revit3DvikSchemas.ViewModels;


namespace Revit3DvikSchemas.Models {

    internal class UniqName {

        public bool WasMet { get; set; }
        public string Name { get; set; }
    }



    internal class ViewMaker {
        private RevitRepository _revitRepository;
        private bool _combineViews;
        private bool _useFopNames;


        public ViewMaker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public List<ElementFilter> CreateElementFilterList(ElementId nameId, List<string> systemNameList) {

            List<ElementFilter> elementFilterList = new List<ElementFilter>();
            foreach(string systemName in systemNameList) {
#if REVIT_2022_OR_LESS
                ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(nameId, systemName, true));
#else
            ElementParameterFilter filterRule = new ElementParameterFilter(ParameterFilterRuleFactory.CreateNotEqualsRule(nameId, systemName));
#endif
                elementFilterList.Add(filterRule);
            }
            return elementFilterList;

        }

        public ElementId getCriterionParamId() {
            ElementId nameId = null;
            if(_useFopNames) {
                ParameterElement parameterElement = _revitRepository.Document.GetSharedParam("ФОП_ВИС_Имя системы");
                nameId = parameterElement.Id;
                ;
            } else {
                nameId = new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            }
            return nameId;

        }

        public ParameterFilterElement CreateFilter(string filterName, List<string> systemNameList) {
            List<ElementId> categories = new List<ElementId>();
            List<BuiltInCategory> allCategories = new List<BuiltInCategory>() {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers
            };

            foreach(BuiltInCategory category in allCategories) {
                categories.Add(new ElementId(category));
            }
            if(_useFopNames) {
                categories.Add(new ElementId(BuiltInCategory.OST_GenericModel));
            }

            //айди параметра по которому мы будем фильтровать
            ElementId criterionParamId = getCriterionParamId();

            //создаем лист из фильтров по именам систем
            List<ElementFilter> elementFilterList = CreateElementFilterList(criterionParamId, systemNameList);

            LogicalAndFilter andFilter = new LogicalAndFilter(elementFilterList);

            ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(_revitRepository.Document, filterName, categories, andFilter);

            return parameterFilterElement;
        }


        public void CreateSelectedCommand(ObservableCollection<HvacSystemViewModel> list, bool useFopNames, bool combineViews) {
            _combineViews = combineViews;
            _useFopNames = useFopNames;
            List<HvacSystemViewModel> systems = new List<HvacSystemViewModel>();
            foreach(HvacSystemViewModel system in list) {
                if(system.IsChecked) {
                    systems.Add(system);
                }
            }

            using(Transaction t = _revitRepository.Document.StartTransaction("Создать схемы")) {
                CopyViewCreateFilters(systems);

                t.Commit();

            }

        }

        private void CopyViewCreateFilters(List<HvacSystemViewModel> systems) {
            if(_combineViews) {
                if(_revitRepository.ActiveUIDocument.ActiveView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {
                    string groupName = MakeGroupViewName(systems, _useFopNames);
                    string filterName = "B4E_" + groupName;

                    List<string> systemNameList = new List<string>();
                    foreach(HvacSystemViewModel system in systems) {
                        string systemOrFopSystemName = GetSystemOrFopSystemName(system, _useFopNames);
                        systemNameList.Add(systemOrFopSystemName);
                    }

                    CopyView(groupName, filterName, systemNameList);

                }
            } else {
                foreach(HvacSystemViewModel system in systems) {
                    string systemOrFopSystemName = GetSystemOrFopSystemName(system, _useFopNames);
                    if(_revitRepository.ActiveUIDocument.ActiveView.CanViewBeDuplicated(ViewDuplicateOption.WithDetailing)) {

                        string filterName = "B4E_" + systemOrFopSystemName;

                        List<string> systemNameList = new List<string>();
                        systemNameList.Add(systemOrFopSystemName);

                        CopyView(systemOrFopSystemName, filterName, systemNameList);
                    }
                }
            }
        }

        private void CopyView(string viewName, string filterName, List<string> systemNameList) {
            FilteredElementCollector collector = new FilteredElementCollector(_revitRepository.Document);
            List<Element> filterCol = (List<Element>) collector.OfClass(typeof(ParameterFilterElement)).ToElements();
            List<Element> viewCol = _revitRepository.GetCollection(BuiltInCategory.OST_Views);


            string uniqFilterName = IsNameUniqOrMakeNew(filterName, filterCol);
            string uniqViewName = IsNameUniqOrMakeNew(viewName, viewCol);

            ElementId newViewId = _revitRepository.ActiveUIDocument.ActiveView.Duplicate(ViewDuplicateOption.WithDetailing);
            View newView = _revitRepository.Document.GetElement(newViewId) as View;
            newView.Name = uniqViewName;
            ParameterFilterElement parameterFilterElement = CreateFilter(uniqFilterName, systemNameList);
            newView.AddFilter(parameterFilterElement.Id);
            newView.SetFilterVisibility(parameterFilterElement.Id, false);

        }

        private static string IsNameUniqOrMakeNew(string name, List<Element> elements) {
            UniqName uniqName = new UniqName();
            uniqName.Name = name;
            uniqName.WasMet = true;

            while(uniqName.WasMet == true) {
                uniqName.WasMet = false;
                foreach(Element element in elements) {
                    if(element.Name == uniqName.Name) {
                        uniqName.Name = uniqName.Name + "_" + "Копия";
                        uniqName.WasMet = true;
                    }
                }
            }
            string newName = uniqName.Name;
            return newName;
        }

        private static string MakeGroupViewName(List<HvacSystemViewModel> systems, bool useFopNames) {
            string name = null;

            foreach(HvacSystemViewModel system in systems) {
                if(useFopNames) {
                    name = name + "_" + system.FopName;

                } else {

                    name = name + "_" + system.SystemName;
                }
            }
            return name;
        }

        private static string GetSystemOrFopSystemName(HvacSystemViewModel system, bool useFopNames) {
            string name = null;

            if(useFopNames) {
                name = system.FopName;

            } else {
                name = system.SystemName;
            }


            return name;

        }
    }
}

