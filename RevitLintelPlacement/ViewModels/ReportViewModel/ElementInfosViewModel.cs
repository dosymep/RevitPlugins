﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class ElementInfosViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ViewOrientation3D _orientation;
        private ObservableCollection<ElementInfoViewModel> _elementIfos;
        private List<TypeInfoViewModel> _types;
        private CollectionViewSource _elementInfosViewSource;
        private ObservableCollection<GroupedDescriptionViewModel> _groupedDescriptions;
        private GroupedDescriptionViewModel _selectedGroupedDescription;
        private CollectionViewSource _descriptionViewSoure;

        public ElementInfosViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            TypeInfos = new List<TypeInfoViewModel>();
            ElementInfos = new ObservableCollection<ElementInfoViewModel>();
            _orientation = _revitRepository.GetOrientation3D();
            SelectElementCommand = new RelayCommand(async p => await SelectElement(p));
            
            InitializeTypeInfo();
            ElementInfosViewSource = new CollectionViewSource() {
                Source = ElementInfos
            };
            DescriptionCheckedCommand = new RelayCommand(DescriptionChecked);
            TypeInfoCheckedCommand = new RelayCommand(TypeInfoChecked);
            //ElementInfosViewSource.Filter += TypeFilter;
            ElementInfosViewSource.Filter += MessageFilter;
        }

        public ICommand SelectElementCommand { get; set; }
        public ICommand TypeInfoCheckedCommand { get; set; }
        public ICommand DescriptionCheckedCommand { get; set; }

        public GroupedDescriptionViewModel SelectedGroupedDescription {
            get => _selectedGroupedDescription;
            set => this.RaiseAndSetIfChanged(ref _selectedGroupedDescription, value);
        }

        public ObservableCollection<ElementInfoViewModel> ElementInfos {
            get => _elementIfos;
            set => this.RaiseAndSetIfChanged(ref _elementIfos, value);
        }

        public ObservableCollection<GroupedDescriptionViewModel> GroupedDescriptions {
            get => _groupedDescriptions;
            set => this.RaiseAndSetIfChanged(ref _groupedDescriptions, value);
        }

        public CollectionViewSource DescriptionViewSoure { 
            get => _descriptionViewSoure; 
            set => this.RaiseAndSetIfChanged(ref _descriptionViewSoure, value); 
        }

        public List<TypeInfoViewModel> TypeInfos {
            get => _types;
            set => this.RaiseAndSetIfChanged(ref _types, value);
        }

        public CollectionViewSource ElementInfosViewSource {
            get => _elementInfosViewSource;
            set => this.RaiseAndSetIfChanged(ref _elementInfosViewSource, value);
        }

        public void UpdateCollection() {
            ElementInfos = new ObservableCollection<ElementInfoViewModel>(ElementInfos
                .Distinct()
                .OrderBy(e=>e.Name));
            ElementInfosViewSource.Source = ElementInfos;
        }

        public void UpdateGroupedMessage() {
            if(ElementInfos.Count > 0) {
                GroupedDescriptions = new ObservableCollection<GroupedDescriptionViewModel>(
                    ElementInfos.GroupBy(item =>
                    new {
                        Type = item.TypeInfo,
                        Message = item.Message
                    })
                    .Select(item => new GroupedDescriptionViewModel() {
                        TypeInfo = item.Key.Type,
                        Message = item.Key.Message
                    }));
                SelectedGroupedDescription = GroupedDescriptions.FirstOrDefault();
            } else {
                GroupedDescriptions = new ObservableCollection<GroupedDescriptionViewModel>();
            }
            DescriptionViewSoure = new CollectionViewSource { Source = GroupedDescriptions };
            DescriptionViewSoure.Filter += TypeFilter;
        }

        private async Task SelectElement(object p) {
            if(p is ElementInfoViewModel elementInfo) {
                if(_revitRepository.GetElementById(elementInfo.ElementId) is Autodesk.Revit.DB.ElementType elementType) {
                    TaskDialog.Show("Revit", "Невозможно подобрать вид для отображения элемента.");
                } else {
                    await _revitRepository.SelectAndShowElement(elementInfo.ElementId, _orientation);
                }
            }
        }

        private void InitializeTypeInfo() {
            foreach(var value in Enum.GetValues(typeof(TypeInfo))) {
                TypeInfos.Add(new TypeInfoViewModel() {
                    TypeInfo = (TypeInfo) value,
                    IsChecked = true
                });
            }
        }

        private void TypeFilter(object sender, FilterEventArgs e) {
            if(e.Item is GroupedDescriptionViewModel description) {
                if(!TypeInfos
                    .Where(t => t.IsChecked)
                    .Any(t => t.TypeInfo == description.TypeInfo)) {
                    e.Accepted = false;
                }

            }
        }

        private void MessageFilter(object sender, FilterEventArgs e) {
            if(SelectedGroupedDescription == null)
                return;
            if(e.Item is ElementInfoViewModel elementInfo) {
                if(!elementInfo.Message.Equals(SelectedGroupedDescription.Message, StringComparison.CurrentCultureIgnoreCase)
                    || elementInfo.TypeInfo != SelectedGroupedDescription.TypeInfo) {
                    e.Accepted = false;
                }
            }
        }

        private void TypeInfoChecked(object p) {
            DescriptionViewSoure.View.Refresh();
        }

        private void DescriptionChecked(object p) {
            ElementInfosViewSource.View.Refresh();
        }
    }

    internal class TypeInfoViewModel : BaseViewModel {
        private bool _isChecked;
        private TypeInfo _typeInfo;

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public TypeInfo TypeInfo {
            get => _typeInfo;
            set => this.RaiseAndSetIfChanged(ref _typeInfo, value);
        }
    }

    internal class GroupedDescriptionViewModel : BaseViewModel {
        private string _message;
        private TypeInfo _typeInfo;

        public TypeInfo TypeInfo {
            get => _typeInfo;
            set => this.RaiseAndSetIfChanged(ref _typeInfo, value);
        }

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

    }
}