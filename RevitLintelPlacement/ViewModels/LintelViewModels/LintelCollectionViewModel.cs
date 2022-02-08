﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Comparers;
using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ElementInWallKind _selectedElementKind;
        private ViewOrientation3D _orientation; //вряд ли здесь нужно хранить
        private ObservableCollection<LintelInfoViewModel> _lintelInfos;
        private SampleMode selectedSampleMode;
        private int countLintelInView;

        public LintelCollectionViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            InitializeLintels(SelectedSampleMode);
            LintelsViewSource = new CollectionViewSource { Source = LintelInfos };
            LintelsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LintelInfoViewModel.Level)));
            LintelsViewSource.Filter += ElementInWallKindFilter;
            SelectAndShowElementCommand = new RelayCommand(SelectElement, p => true);
            SelectNextCommand = new RelayCommand(SelectNext, p => true);
            SelectPreviousCommand = new RelayCommand(SelectPrevious, p => true);
            SelectionElementKindChangedCommand = new RelayCommand(SelectionElementKindChanged, p => true);
            SampleModeChangedCommand = new RelayCommand(SampleModeChanged, p => true);
            CountLintelInView = LintelsViewSource.View.Cast<LintelInfoViewModel>().Count();
        }

        public int CountLintelInView { 
            get => countLintelInView; 
            set => this.RaiseAndSetIfChanged(ref countLintelInView, value); 
        }

        public ElementInWallKind SelectedElementKind {
            get => _selectedElementKind;
            set => this.RaiseAndSetIfChanged(ref _selectedElementKind, value);
        }

        public SampleMode SelectedSampleMode {
            get => selectedSampleMode;
            set => this.RaiseAndSetIfChanged(ref selectedSampleMode, value);
        }

        public ICommand SelectionElementKindChangedCommand { get; }

        public ICommand SelectAndShowElementCommand { get; }

        public ICommand SelectNextCommand { get; }

        public ICommand SelectPreviousCommand { get; }

        public ICommand SampleModeChangedCommand { get; set; }

        public CollectionViewSource LintelsViewSource { get; set; }

        public ObservableCollection<LintelInfoViewModel> LintelInfos {
            get => _lintelInfos;
            set => this.RaiseAndSetIfChanged(ref _lintelInfos, value);
        }

        private void ElementInWallKindFilter(object sender, FilterEventArgs e) {
            if(e.Item is LintelInfoViewModel lintel) {
                e.Accepted = SelectedElementKind == ElementInWallKind.All ? true : lintel.ElementInWallKind == SelectedElementKind;
            }
        }

        private void SelectElement(object p) {
            if(!(p is ElementId id) || p == null) {
                return;
            }
            _revitRepository.SetActiveView();
            if(_orientation == null) {
                _orientation = _revitRepository.GetOrientation3D();
            }
            _revitRepository.SelectAndShowElement(id, _orientation);
        }

        private void SelectNext(object p) {
            LintelsViewSource.View.MoveCurrentToNext();
            var lintelInfo = LintelsViewSource.View.CurrentItem;
            if(lintelInfo is LintelInfoViewModel lintel) {
                SelectElement(lintel.LintelId);
            }
        }

        private void SelectPrevious(object p) {
            LintelsViewSource.View.MoveCurrentToPrevious();
            var lintelInfo = LintelsViewSource.View.CurrentItem;
            if(lintelInfo is LintelInfoViewModel lintel) {
                SelectElement(lintel.LintelId);
            }
        }

        private void SelectionElementKindChanged(object p) {
            LintelsViewSource.View.Refresh();
            CountLintelInView = LintelsViewSource.View.Cast<LintelInfoViewModel>().Count();
        }

        //сопоставляются перемычки в группе + перемычки, закрепленные с элементом
        private void InitializeLintels(SampleMode sampleMode) {
            LintelInfos = new ObservableCollection<LintelInfoViewModel>();
            var lintels = _revitRepository.GetLintels(sampleMode);
            var correlator = new LintelElementCorrelator(_revitRepository);
            var lintelInfos = lintels
                .Select(l => new LintelInfoViewModel(_revitRepository, l, correlator.Correlate(l)))
                .OrderBy(l => l.Level, new AlphanumericComparer());
            foreach(var lintelInfo in lintelInfos) {
                LintelInfos.Add(lintelInfo);
            }
        }

        private void SampleModeChanged(object p) {
            InitializeLintels(SelectedSampleMode);
            LintelsViewSource.Source = LintelInfos;
            LintelsViewSource.View.Refresh();
            CountLintelInView = LintelsViewSource.View.Cast<LintelInfoViewModel>().Count();
        }
    }

    internal enum SampleMode {
        [Description("Выборка по всем элементам")]
        AllElements,
        [Description("Выборка по выделенным элементам")]
        SelectedElements,
        [Description("Выборка по текущему виду")]
        CurrentView
    }
}
