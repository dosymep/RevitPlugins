﻿using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace Revit3DvikSchemas.ViewModels {
    internal class HvacSystemViewModel : BaseViewModel {
        public string SystemName { get; set; }
        public string FopName { get; set; }
        public Element SystemElement { get; set; }

        private bool _isChecked;


        public bool IsChecked {
            get { return _isChecked; }
            set {
                _isChecked = value;
                OnPropertyChanged();
                //RaisePropertyChanged("_isChecked");
            }
        }
    }
}
