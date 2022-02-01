﻿using System;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;

using RevitSetLevelSection.Models;
using RevitSetLevelSection.ViewModels;
using RevitSetLevelSection.Views;

namespace RevitSetLevelSection {
    [Transaction(TransactionMode.Manual)]
    public class SetLevelSectionCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var repository = new RevitRepository(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document);
                var viewModel = new MainViewModel(repository);

                var window = new MainWindow() { DataContext = viewModel };
                new WindowInteropHelper(window) { Owner = commandData.Application.MainWindowHandle };

                window.ShowDialog();
            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Назначение уровня/секции.", ex.ToString());
#else
                TaskDialog.Show("Назначение уровня/секции.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }
}