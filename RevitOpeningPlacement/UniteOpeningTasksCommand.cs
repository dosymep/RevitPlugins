﻿using System.Collections.Generic;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Класс команды для объединения исходящих заданий на отверстия
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class UniteOpeningTasksCommand : BasePluginCommand {

        protected override void Execute(UIApplication uiApplication) {
            RevitRepository revitRepository = new RevitRepository(uiApplication.Application, uiApplication.ActiveUIDocument.Document);
            ICollection<OpeningMepTaskOutcoming> openingTasks = revitRepository.PickManyOpeningTasksOutcoming();

            var placedOpeningTask = revitRepository.UniteOpenings(openingTasks);
            uiApplication.ActiveUIDocument.Selection.SetElementIds(new ElementId[] { placedOpeningTask.Id });
        }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }
    }
}