﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.SearchSet {
    internal class GridControlViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<IFilterableValueProvider> _providers;
#if REVIT_2023_OR_LESS
        private readonly List<int> _categoryIds;
#else
        private readonly List<long> _categoryIds;
#endif
        private readonly IEnumerable<Element> _elements;
        private int _elementsCount;

        public GridControlViewModel(RevitRepository revitRepository, Filter filter, IEnumerable<Element> elements) {
            _revitRepository = revitRepository;
            _providers = filter.GetProviders();
            _categoryIds = filter.CategoryIds;
            _elements = elements;
            InitializeColumns();
            InitializeRows();
            AddCommonInfo();

            SelectCommand = new RelayCommand(p => SelectElement(p));
        }

        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ObservableCollection<ExpandoObject> Rows { get; set; }

        public ICommand SelectCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand RenewCountCommand { get; }
        public int ElementsCount {
            get => _elementsCount;
            set => this.RaiseAndSetIfChanged(ref _elementsCount, value);
        }

        private void InitializeColumns() {
            Columns = new ObservableCollection<ColumnViewModel>(
                _providers.Select(item => new ColumnViewModel() {
                    FieldName = item.Name,
                    Header = $"Параметр: {item.DisplayValue}"
                }).GroupBy(item => item.FieldName)
                .Select(item => item.First()));
        }

        private void InitializeRows() {
            Rows = new ObservableCollection<ExpandoObject>();
            foreach(var element in _elements) {
                IDictionary<string, object> row = new ExpandoObject();
                foreach(var provider in _providers) {
                    string value = provider.GetElementParamValue(element)?.DisplayValue;
                    if((provider.StorageType == StorageType.Integer || provider.StorageType == StorageType.Double)
                        && double.TryParse(value, out double resultValue)
                        && resultValue != 0) {
                        AddValue(row, provider.Name, resultValue);
                    } else if(!string.IsNullOrEmpty(value) && value != "0") {
                        AddValue(row, provider.Name, value);
                    }

                }
                row.Add("File", _revitRepository.GetDocumentName(element.Document));
                row.Add("Category", element.Category?.Name);
                row.Add("FamilyName", element.GetTypeId().IsNotNull() ? (element.Document.GetElement(element.GetTypeId()) as ElementType)?.FamilyName : null);
                row.Add("Name", element.Name);
#if REVIT_2023_OR_LESS
                row.Add("Id", element.Id.IntegerValue);
#else
                row.Add("Id", element.Id.GetIdValue());
#endif
                Rows.Add((ExpandoObject) row);
            }
        }

        private void AddValue<T>(IDictionary<string, object> row, string key, T value) {
            if(!row.ContainsKey(key)) {
                row.Add(key, value);
            } else {
                row[key] = value;
            }
        }

        private void AddCommonInfo() {
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Name", Header = "Имя типоразмера" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "FamilyName", Header = "Имя семейства" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Category", Header = "Категория" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "Id", Header = "Id" });
            Columns.Insert(0, new ColumnViewModel() { FieldName = "File", Header = "Файл" });
        }

        private void SelectElement(object p) {
            if(!(p is ExpandoObject row))
                return;
            ((IDictionary<string, object>) row).TryGetValue("Id", out object resultId);
            ((IDictionary<string, object>) row).TryGetValue("File", out object resultFile);
            if(resultId == null) {
                return;
            }
#if REVIT_2023_OR_LESS
            if(resultId is int id) {
                var element = GetElement(id, resultFile.ToString());
                if(element != null) {
                    _revitRepository.SelectAndShowElement(new[] { element });
                }
            }
#else
            if(resultId is long id) {
                var element = GetElement(id, resultFile.ToString());
                if(element != null) {
                    _revitRepository.SelectAndShowElement(new[] { element });
                }
            }
#endif
        }

#if REVIT_2023_OR_LESS
        private Element GetElement(int id, string documentName) {
            var doc = _revitRepository.GetDocuments()
                .FirstOrDefault(item => _revitRepository.GetDocumentName(item).Equals(documentName, StringComparison.CurrentCultureIgnoreCase));
            if(doc != null && new ElementId(id).IsNotNull()) {
                return doc.GetElement(new ElementId(id));
            }
            return null;
        }
#else
        private Element GetElement(long id, string documentName) {
            var doc = _revitRepository.GetDocuments()
                .FirstOrDefault(item => _revitRepository.GetDocumentName(item).Equals(documentName, StringComparison.CurrentCultureIgnoreCase));
            if(doc != null && new ElementId(id).IsNotNull()) {
                return doc.GetElement(new ElementId(id));
            }
            return null;
        }
#endif
    }
}
