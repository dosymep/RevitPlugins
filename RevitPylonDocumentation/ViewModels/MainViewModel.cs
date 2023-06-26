﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using Autodesk.AdvanceSteel.Modelling;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
//using Autodesk.SteelConnectionsDB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using MS.WindowsAPICodePack.Internal;

using RevitPylonDocumentation.Models;

using Transform = Autodesk.Revit.DB.Transform;
using View = Autodesk.Revit.DB.View;
using Wall = Autodesk.Revit.DB.Wall;

namespace RevitPylonDocumentation.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        public readonly RevitRepository _revitRepository;

        /// <summary>
        /// Указывает вносил ли пользователь изменения в настройки
        /// </summary>
        private bool _edited = false;
        private string _errorText;


        private string _selectedProjectSection = string.Empty;
        private string _hostMarkForSearch = string.Empty;


        private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
        private string _markTemp = "Марка";
        private string _sheetGroupingTemp = "_Группа видов 1";
        private string _sheetSizeTemp = "А";
        private string _sheetCoefficientTemp = "х";

        private string _sheetPrefixTemp = "Пилон ";
        private string _sheetSuffixTemp = "";

        private string _generalViewPrefixTemp = "";
        private string _generalViewSuffixTemp = "";
        private string _generalViewPerpendicularPrefixTemp = "Пилон ";
        private string _generalViewPerpendicularSuffixTemp = "_Перпендикулярный";
        private string _transverseViewFirstPrefixTemp = "";
        private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
        private string _transverseViewSecondPrefixTemp = "";
        private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
        private string _transverseViewThirdPrefixTemp = "";
        private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";

        private string _rebarSchedulePrefixTemp = "Пилон ";
        private string _rebarScheduleSuffixTemp = "";
        private string _materialSchedulePrefixTemp = "!СМ_Пилон ";
        private string _materialScheduleSuffixTemp = "";
        private string _partsSchedulePrefixTemp = "!ВД_IFC_";
        private string _partsScheduleSuffixTemp = "";
        
        public static string DEF_TITLEBLOCK_NAME = "Создать типы по комплектам";

        private List<PylonSheetInfo> _selectedHostsInfo = new List<PylonSheetInfo>();




        /// <summary>
        /// Инфо про существующие в проекте листы пилонов
        /// </summary>
        public Dictionary<string, PylonSheetInfo> existingPylonSheetsInfo = new Dictionary<string, PylonSheetInfo>();
        /// <summary>
        /// Инфо про листы пилонов, которые нужно создать
        /// </summary>
        public Dictionary<string, PylonSheetInfo> missingPylonSheetsInfo = new Dictionary<string, PylonSheetInfo>();
        // Вспомогательный список
        public Dictionary<string, List<FamilyInstance>> hostData = new Dictionary<string, List<FamilyInstance>>();

        public ReportHelper reportHelper = new ReportHelper();


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            GetRebarProjectSections();

            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;

            TitleBlocks = _revitRepository.TitleBlocksInProject;
            SelectedTitleBlocks = TitleBlocks
                .FirstOrDefault(titleBlock => titleBlock.Name == DEF_TITLEBLOCK_NAME);

            Legends = _revitRepository.LegendsInProject;
            SelectedLegend = Legends
                .FirstOrDefault(view => view.Name.Contains("илон"));


            GetHostMarksInGUICommand = new RelayCommand(GetHostMarksInGUI);


            TestCommand = new RelayCommand(Test);

            CreateSheetsCommand = new RelayCommand(CreateSheets, CanCreateSheets);
            ApplySettingsCommands = new RelayCommand(ApplySettings, CanApplySettings);
        }



        public ICommand ApplySettingsCommands { get; }
        public ICommand CreateSheetsCommand { get; }
        public ICommand GetHostMarksInGUICommand { get; }
        public ICommand TestCommand { get; }



        /// <summary>
        /// Список всех комплектов документации (по ум. обр_ФОП_Раздел проекта)
        /// </summary>
        public ObservableCollection<string> ProjectSections { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<PylonSheetInfo> HostsInfo { get; set; } = new ObservableCollection<PylonSheetInfo>();





        /// <summary>
        /// Выбранный пользователем комплект документации
        /// </summary>
        public string SelectedProjectSection {
            get => _selectedProjectSection;
            set => this.RaiseAndSetIfChanged(ref _selectedProjectSection, value);
        }


        /// <summary>
        /// Выбранный пользователем комплект документации
        /// </summary>
        public List<PylonSheetInfo> SelectedHostsInfo {
            get => _selectedHostsInfo;
            set => this.RaiseAndSetIfChanged(ref _selectedHostsInfo, value);
        }




        // Вспомогательные для документации
        /// <summary>
        /// Рамки листов, имеющиеся в проекте
        /// </summary>
        public List<FamilySymbol> TitleBlocks { get; set; } = new List<FamilySymbol>();
        /// <summary>
        /// Выбранная пользователем рамка листа
        /// </summary>
        public FamilySymbol SelectedTitleBlocks { get; set; }
        /// <summary>
        /// Легенды, имеющиеся в проекте
        /// </summary>
        public List<View> Legends { get; set; } = new List<View>();
        /// <summary>
        /// Выбранная пользователем легенда
        /// </summary>
        public static View SelectedLegend { get; set; }
        /// <summary>
        /// Типоразмеры видов, имеющиеся в проекте
        /// </summary>
        public List<ViewFamilyType> ViewFamilyTypes { get; set; } = new List<ViewFamilyType>();
        /// <summary>
        /// Выбранный пользователем типоразмер вида для создания новых видов
        /// </summary>
        public static ViewFamilyType SelectedViewFamilyType { get; set; }


        // Инфо по пилонам
        /// <summary>
        /// Список всех марок пилонов (напр., "12.30.25-20⌀32")
        /// </summary>
        public ObservableCollection<string> HostMarks { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// Список меток основ, которые выбрал пользователь
        /// </summary>
        public System.Collections.IList SelectedHostMarks { get; set; }

        public string HostMarkForSearch {
            get => _hostMarkForSearch;
            set {
                _hostMarkForSearch = value;
            }
        }



        #region Свойства для параметров и правил
        #region Параметры
        public string PROJECT_SECTION { get; set; } = "обр_ФОП_Раздел проекта";
        public string PROJECT_SECTION_TEMP {
            get => _projectSectionTemp;
            set {
                _projectSectionTemp = value;
                _edited = true;
            }
        }

        public string MARK { get; set; } = "Марка";
        public string MARK_TEMP {
            get => _markTemp;
            set {
                _markTemp = value;
                _edited = true;
            }
        }


        public string SHEET_GROUPING { get; set; } = "_Группа видов 1";
        public string SHEET_GROUPING_TEMP {
            get => _sheetGroupingTemp;
            set {
                _sheetGroupingTemp = value;
                _edited = true;
            }
        }


        public string SHEET_SIZE { get; set; } = "А";
        public string SHEET_SIZE_TEMP {
            get => _sheetSizeTemp;
            set {
                _sheetSizeTemp = value;
                _edited = true;
            }
        }

        public string SHEET_COEFFICIENT { get; set; } = "х";
        public string SHEET_COEFFICIENT_TEMP {
            get => _sheetCoefficientTemp;
            set {
                _sheetCoefficientTemp = value;
                _edited = true;
            }
        }


        public string SHEET_PREFIX { get; set; } = "Пилон ";
        public string SHEET_PREFIX_TEMP {
            get => _sheetPrefixTemp;
            set {
                _sheetPrefixTemp = value;
                _edited = true;
            }
        }

        public string SHEET_SUFFIX { get; set; } = "";
        public string SHEET_SUFFIX_TEMP {
            get => _sheetSuffixTemp;
            set {
                _sheetSuffixTemp = value;
                _edited = true;
            }
        }



        public string GENERAL_VIEW_PREFIX { get; set; } = "";
        public string GENERAL_VIEW_PREFIX_TEMP {
            get => _generalViewPrefixTemp;
            set {
                _generalViewPrefixTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_SUFFIX { get; set; } = "";
        public string GENERAL_VIEW_SUFFIX_TEMP {
            get => _generalViewSuffixTemp;
            set {
                _generalViewSuffixTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_PERPENDICULAR_PREFIX { get; set; } = "Пилон ";
        public string GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP {
            get => _generalViewPerpendicularPrefixTemp;
            set {
                _generalViewPerpendicularPrefixTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX { get; set; } = "_Перпендикулярный";
        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP {
            get => _generalViewPerpendicularSuffixTemp;
            set {
                _generalViewPerpendicularSuffixTemp = value;
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_FIRST_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_FIRST_PREFIX_TEMP {
            get => _transverseViewFirstPrefixTemp;
            set {
                _transverseViewFirstPrefixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_FIRST_SUFFIX { get; set; } = "_Сеч.1-1";
        public string TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP {
            get => _transverseViewFirstSuffixTemp;
            set {
                _transverseViewFirstSuffixTemp = value;
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_SECOND_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_SECOND_PREFIX_TEMP {
            get => _transverseViewSecondPrefixTemp;
            set {
                _transverseViewSecondPrefixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_SECOND_SUFFIX { get; set; } = "_Сеч.2-2";
        public string TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP {
            get => _transverseViewSecondSuffixTemp;
            set {
                _transverseViewSecondSuffixTemp = value;
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_THIRD_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_THIRD_PREFIX_TEMP {
            get => _transverseViewThirdPrefixTemp;
            set {
                _transverseViewThirdPrefixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_THIRD_SUFFIX { get; set; } = "_Сеч.3-3";
        public string TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP {
            get => _transverseViewThirdSuffixTemp;
            set {
                _transverseViewThirdSuffixTemp = value;
                _edited = true;
            }
        }




        public string REBAR_SCHEDULE_PREFIX { get; set; } = "Пилон ";
        public string REBAR_SCHEDULE_PREFIX_TEMP {
            get => _rebarSchedulePrefixTemp;
            set {
                _rebarSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string REBAR_SCHEDULE_SUFFIX { get; set; } = "";
        public string REBAR_SCHEDULE_SUFFIX_TEMP {
            get => _rebarScheduleSuffixTemp;
            set {
                _rebarScheduleSuffixTemp = value;
                _edited = true;
            }
        }


        public string MATERIAL_SCHEDULE_PREFIX { get; set; } = "!СМ_Пилон ";
        public string MATERIAL_SCHEDULE_PREFIX_TEMP {
            get => _materialSchedulePrefixTemp;
            set {
                _materialSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string MATERIAL_SCHEDULE_SUFFIX { get; set; } = "";
        public string MATERIAL_SCHEDULE_SUFFIX_TEMP {
            get => _materialScheduleSuffixTemp;
            set {
                _materialScheduleSuffixTemp = value;
                _edited = true;
            }
        }


        public string PARTS_SCHEDULE_PREFIX { get; set; } = "!ВД_IFC_";
        public string PARTS_SCHEDULE_PREFIX_TEMP {
            get => _partsSchedulePrefixTemp;
            set {
                _partsSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string PARTS_SCHEDULE_SUFFIX { get; set; } = "";
        public string PARTS_SCHEDULE_SUFFIX_TEMP {
            get => _partsScheduleSuffixTemp;
            set {
                _partsScheduleSuffixTemp = value;
                _edited = true;
            }
        }

#endregion


        public string Report {
            get => reportHelper.GetAsString();
            set {
                reportHelper.AppendLine(value);
                this.OnPropertyChanged();
            }
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }




        // Получаем названия Комплектов документации по пилонам
        private void GetRebarProjectSections() 
        {
            // Пользователь может перезадать параметр раздела, поэтому сначала чистим
            ProjectSections.Clear();
            ErrorText = string.Empty;

            _revitRepository.GetHostData(this);


            HostsInfo = new ObservableCollection<PylonSheetInfo>(_revitRepository.HostsInfo);
            ProjectSections = new ObservableCollection<string>(_revitRepository.HostProjectSections);
            OnPropertyChanged(nameof(HostsInfo));
            OnPropertyChanged(nameof(ProjectSections));
        }


        // Метод для авто обновления списка марок пилонов при выборе рабочего набора
        private void GetHostMarksInGUI(object p) 
        {
            ErrorText= string.Empty;

            SelectedHostsInfo = new List<PylonSheetInfo>(HostsInfo
                .Where(item => item.ProjectSection.Equals(SelectedProjectSection))
                .ToList());
        }




        private void ApplySettings(object p) {
            // Необходимо будет написать метод проверки имен параметров - есть ли такие параметры у нужных категорий
            

            // Устанавливаем флаг, что применили параметры и перезаписываем параметры
            PROJECT_SECTION = _projectSectionTemp;
            MARK = _markTemp;
            SHEET_GROUPING= _sheetGroupingTemp;
            SHEET_SIZE = _sheetSizeTemp;
            SHEET_COEFFICIENT = _sheetCoefficientTemp;
            GENERAL_VIEW_PREFIX = _generalViewPrefixTemp;
            GENERAL_VIEW_SUFFIX = _generalViewSuffixTemp;
            GENERAL_VIEW_PERPENDICULAR_PREFIX = _generalViewPerpendicularPrefixTemp;
            GENERAL_VIEW_PERPENDICULAR_SUFFIX = _generalViewPerpendicularSuffixTemp;
            TRANSVERSE_VIEW_FIRST_PREFIX = _transverseViewFirstPrefixTemp;
            TRANSVERSE_VIEW_FIRST_SUFFIX = _transverseViewFirstSuffixTemp;
            TRANSVERSE_VIEW_SECOND_PREFIX = _transverseViewSecondPrefixTemp;
            TRANSVERSE_VIEW_SECOND_SUFFIX = _transverseViewSecondSuffixTemp;
            TRANSVERSE_VIEW_THIRD_PREFIX = _transverseViewThirdPrefixTemp;
            TRANSVERSE_VIEW_THIRD_SUFFIX = _transverseViewThirdSuffixTemp;
            REBAR_SCHEDULE_PREFIX = _rebarSchedulePrefixTemp;
            REBAR_SCHEDULE_SUFFIX = _rebarScheduleSuffixTemp;
            MATERIAL_SCHEDULE_PREFIX = _materialSchedulePrefixTemp;
            MATERIAL_SCHEDULE_SUFFIX = _materialScheduleSuffixTemp;
            PARTS_SCHEDULE_PREFIX = _partsSchedulePrefixTemp;
            PARTS_SCHEDULE_SUFFIX = _partsScheduleSuffixTemp;

            _edited = false;

            // Получаем заново список заполненных разделов проекта
            GetRebarProjectSections();
        }
        private bool CanApplySettings(object p) {
            if(_edited) {
                return true;
            }
            return false;
        }



        private void CreateSheets(object p) {
            // Забираем список выбранных элементов через CommandParameter
            SelectedHostMarks = p as System.Collections.IList;

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<string> selectedHostMarks = new List<string>();
            foreach(var item in SelectedHostMarks) {
                string hostMark = item as string;
                if(hostMark == null) {
                    continue;
                }
                selectedHostMarks.Add(hostMark);
            }


            string report = string.Empty;
            // Получаем инфо о листах, которые нужно создать
            #region Отчет
            Report = "Приступаем к созданию документации по выбранным пилонам";
            Report = "Анализируем...";
            #endregion
            AnalyzeExistingSheets();



            // Проверка,если ли листы для создания
            if(missingPylonSheetsInfo.Keys.Count > 0) {
                #region Отчет
                Report = "Пользователь выбрал типоразмер рамки листа: " + SelectedTitleBlocks.Name;
                Report = "Пользователь выбрал легенду примечаний: " + SelectedLegend.Name;
                Report = Environment.NewLine + "Приступаю к созданию листов";
                #endregion
            } else {
                #region Отчет
                Report = "Все запрошенные листы уже созданы";
                Report = "Работа завершена";
                #endregion
            }


            Transaction transaction = new Transaction(_revitRepository.Document, "Создание листов пилонов");
            transaction.Start();


            foreach(string sheetKeyName in missingPylonSheetsInfo.Keys) {
                // Лист 
                #region Отчет
                Report = Environment.NewLine + 
                    "---------------------------------------------------------------------------" +
                    "---------------------------------------------------------------------------";
                Report = " - Пилон " + sheetKeyName;
                Report = "\tЛист создан";
                #endregion

                ViewSheet viewSheet = ViewSheet.Create(_revitRepository.Document, SelectedTitleBlocks.Id);
                viewSheet.Name = "Пилон " + sheetKeyName;


                Autodesk.Revit.DB.Parameter viewSheetGroupingParameter = viewSheet.LookupParameter(SHEET_GROUPING);
                if(viewSheetGroupingParameter == null) {
                    #region Отчет
                    Report = "\tПараметр \"" + SHEET_GROUPING + "\" не заполнен, т.к. не был найден у листа";
                    #endregion
                } else {
                    viewSheetGroupingParameter.Set(SelectedProjectSection);
                    #region Отчет
                    Report = "\tГруппировка по параметру \"" + SHEET_GROUPING + "\": " + SelectedProjectSection;
                    #endregion
                }


                // Фиксируем инфо про легенду
                missingPylonSheetsInfo[sheetKeyName].LegendView.ViewElement = SelectedLegend;
                missingPylonSheetsInfo[sheetKeyName].LegendView.ViewportTypeName = "Без названия";
                missingPylonSheetsInfo[sheetKeyName].LegendView.ViewportCenter = new XYZ(-0.34, 0.30, 0);

                missingPylonSheetsInfo[sheetKeyName].PylonViewSheet = viewSheet;

                // Рамка листа
                FamilyInstance titleBlock = new FilteredElementCollector(_revitRepository.Document, viewSheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .FirstElement() as FamilyInstance;

                missingPylonSheetsInfo[sheetKeyName].TitleBlock = titleBlock;

                // Пытаемся задать габарит листа А3 и получаем габариты рамки
                missingPylonSheetsInfo[sheetKeyName].SetTitleBlockSize(_revitRepository.Document);


                // Размещение видовых экранов
                //missingPylonSheetsInfo[sheetKeyName].PlaceGeneralViewport();
                //missingPylonSheetsInfo[sheetKeyName].PlaceTransverseViewPorts();

                // Размещение спецификаций
                missingPylonSheetsInfo[sheetKeyName].PlaceRebarSchedule();
                missingPylonSheetsInfo[sheetKeyName].PlaceMaterialSchedule();
                missingPylonSheetsInfo[sheetKeyName].PlacePartsSchedule();


                // Размещение легенды
                missingPylonSheetsInfo[sheetKeyName].PlaceLegend(_revitRepository);
            }

            transaction.Commit();
        }
        private bool CanCreateSheets(object p) {
            if(ErrorText.Length > 0) {
                return false;
            }
            // Проверяем правила поиска видов на уникальность
            string genView = GENERAL_VIEW_PREFIX + GENERAL_VIEW_SUFFIX;
            string tranViewFirst = TRANSVERSE_VIEW_FIRST_PREFIX + TRANSVERSE_VIEW_FIRST_SUFFIX;
            string tranViewSecond = TRANSVERSE_VIEW_SECOND_PREFIX + TRANSVERSE_VIEW_SECOND_SUFFIX;
            string tranViewThird = TRANSVERSE_VIEW_THIRD_PREFIX + TRANSVERSE_VIEW_THIRD_SUFFIX;

            if(genView == tranViewFirst || genView == tranViewSecond || genView == tranViewThird || tranViewFirst == tranViewSecond
                || tranViewFirst == tranViewThird || tranViewSecond == tranViewThird) {
                ErrorText = "Правила поиска видов некорректны. Задайте уникальные правила в настройках";
                return false;
            }

            // Проверяем правила поиска спек на уникальность
            string rebSchedule = REBAR_SCHEDULE_PREFIX + REBAR_SCHEDULE_SUFFIX;
            string matSchedule = MATERIAL_SCHEDULE_PREFIX + MATERIAL_SCHEDULE_SUFFIX;
            string partsSchedule = PARTS_SCHEDULE_PREFIX + PARTS_SCHEDULE_SUFFIX;

            if(rebSchedule == matSchedule || rebSchedule == partsSchedule || matSchedule == partsSchedule) {
                ErrorText = "Правила поиска спецификаций некорректны. Задайте уникальные правила в настройках";
                return false;
            }

            return true;
        }


        public void AnalyzeExistingSheets() {
            var allExistingSheets = new FilteredElementCollector(_revitRepository.Document)
                .OfClass(typeof(ViewSheet))
                .WhereElementIsNotElementType()
                .ToElements();

            Report = "В проекте найдено листов: " + allExistingSheets.Count.ToString();

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<string> selectedHostMarks = new List<string>();
            foreach(var item in SelectedHostMarks) {
                string hostMark = item as string;
                if(hostMark == null) {
                    continue;
                }

                selectedHostMarks.Add(hostMark);
            }

            
            // Формируем список листов, выбранных для обработки, но уже существущих - existingPylonSheetsInfo
            foreach(var item in allExistingSheets) {
                ViewSheet sheet = item as ViewSheet;
                string sheetKeyName;
                
                if(sheet == null || !sheet.Name.Contains("Пилон") || !sheet.Name.Contains(" ") || sheet.Name.Split(' ').Length < 2) {
                    continue;
                } else {
                    sheetKeyName = sheet.Name.Split(' ')[1];
                }

                if(selectedHostMarks.Contains(sheetKeyName)) {
                    existingPylonSheetsInfo.Add(sheetKeyName, new PylonSheetInfo(this, _revitRepository, sheetKeyName) {
                        PylonViewSheet = sheet,
                    });
                    selectedHostMarks.Remove(sheetKeyName);     // Удаляем имена листов, которые уже есть
                }
            }
            Report = "Из них выбрано для обработки среди уже существующих: " + existingPylonSheetsInfo.Count.ToString();
            if(existingPylonSheetsInfo.Count > 0) {
                Report = "Среди которых: ";
                foreach(string sheetName in existingPylonSheetsInfo.Keys) {
                    Report = " - " + sheetName;
                }
            }


            // Формируем список листов, выбранных для обработки и еще не созданных - wbGeneratingPylonSheetsInfo
            if(selectedHostMarks.Count > 0) {
                foreach(string hostMark in selectedHostMarks) {
                    missingPylonSheetsInfo.Add(hostMark, new PylonSheetInfo(this, _revitRepository, hostMark));
                }
            }
            Report = "Будут созданы листы в количестве: " + missingPylonSheetsInfo.Count.ToString();
            if(missingPylonSheetsInfo.Count > 0) {
                Report = "Среди которых: ";
                foreach(string sheetName in missingPylonSheetsInfo.Keys) {
                    Report = " - " + sheetName;
                }
            }
        }













        private void Test(object p) {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление видов")) {


                foreach(PylonSheetInfo hostsInfo in SelectedHostsInfo) {
                    //try {

                    //} catch(Exception) {}


                    if(!hostsInfo.IsCheck) { continue; }

                    if(hostsInfo.SheetInProjectEditableInGUI && hostsInfo.SheetInProject) {

                        hostsInfo.CreateSheet();
                    }

                    if(hostsInfo.GeneralView.InProjectEditableInGUI && hostsInfo.GeneralView.InProject) {

                        hostsInfo.GeneralView.ViewCreator.CreateGeneralView(SelectedViewFamilyType);
                    }

                    if(hostsInfo.GeneralViewPerpendicular.InProjectEditableInGUI && hostsInfo.GeneralViewPerpendicular.InProject) {

                        hostsInfo.GeneralViewPerpendicular.ViewCreator.CreateGeneralPerpendicularView(SelectedViewFamilyType);
                    }

                    if(hostsInfo.TransverseViewFirst.InProjectEditableInGUI && hostsInfo.TransverseViewFirst.InProject) {

                        hostsInfo.TransverseViewFirst.ViewCreator.CreateTransverseView(SelectedViewFamilyType, 1);
                    }

                    if(hostsInfo.TransverseViewSecond.InProjectEditableInGUI && hostsInfo.TransverseViewSecond.InProject) {

                        hostsInfo.TransverseViewSecond.ViewCreator.CreateTransverseView(SelectedViewFamilyType, 2);
                    }

                    if(hostsInfo.TransverseViewThird.InProjectEditableInGUI && hostsInfo.TransverseViewThird.InProject) {

                        hostsInfo.TransverseViewThird.ViewCreator.CreateTransverseView(SelectedViewFamilyType, 3);
                    }



                    if(hostsInfo.GeneralView.OnSheetEditableInGUI && hostsInfo.GeneralView.OnSheet) {

                        hostsInfo.GeneralView.ViewPlacer.PlaceGeneralViewport();
                    }

                    if(hostsInfo.GeneralViewPerpendicular.OnSheetEditableInGUI && hostsInfo.GeneralViewPerpendicular.OnSheet) {

                        hostsInfo.GeneralViewPerpendicular.ViewPlacer.PlaceGeneralPerpendicularViewport();
                    }

                    if(hostsInfo.TransverseViewFirst.OnSheetEditableInGUI && hostsInfo.TransverseViewFirst.OnSheet) {

                        hostsInfo.TransverseViewFirst.ViewPlacer.PlaceTransverseFirstViewPorts();
                    }

                    if(hostsInfo.TransverseViewSecond.OnSheetEditableInGUI && hostsInfo.TransverseViewSecond.OnSheet) {

                        hostsInfo.TransverseViewSecond.ViewPlacer.PlaceTransverseSecondViewPorts();
                    }

                    if(hostsInfo.TransverseViewThird.OnSheetEditableInGUI && hostsInfo.TransverseViewThird.OnSheet) {

                        hostsInfo.TransverseViewThird.ViewPlacer.PlaceTransverseThirdViewPorts();
                    }
                }



                transaction.Commit();
            }


        }



    }



    public class BooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if(value is true) { return false; } else { return true; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            
            if(value is true) { return false; } else { return true; }
        }
    }
}