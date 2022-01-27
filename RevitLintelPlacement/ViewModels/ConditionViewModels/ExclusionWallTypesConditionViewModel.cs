﻿using System;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class ExclusionWallTypesConditionViewModel : BaseViewModel, IConditionViewModel {
        private ObservableCollection<WallTypeConditionViewModel> _wallTypes;

        public ObservableCollection<WallTypeConditionViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

        public bool Check(FamilyInstance elementInWall) {

            if(elementInWall == null || elementInWall.Id == ElementId.InvalidElementId)
                throw new ArgumentNullException(nameof(elementInWall));

            if(elementInWall.Host == null || !(elementInWall.Host is Wall wall))
                return false;
                //throw new ArgumentNullException(nameof(elementInWall), "На проверку передан некорректный элемент.");

            return !WallTypes.Any(wt => wall.WallType.Name.Contains(wt.Name)); //TODO: или равно (уточнить)
        }
    }
}
