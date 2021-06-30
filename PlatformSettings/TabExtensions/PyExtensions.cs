﻿#region Namespaces
using System;
using System.Collections.Generic;

using PlatformSettings.TabExtensions;

using pyRevitLabs.PyRevit;

#endregion

namespace PlatformSettings.TabExtensions {
    internal class PyExtensions : Extensions {
        public override string Path { get; } = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit-Master", "Extensions");

        public override List<string> Url { get; } = new List<string>() {
            "https://raw.githubusercontent.com/eirannejad/pyRevit/master/extensions/pyRevitTags.extension/extension.json",
            "https://raw.githubusercontent.com/eirannejad/pyRevit/master/extensions/pyRevitTools.extension/extension.json",
        };

        protected override PyRevitExtensionViewModel GetPyRevitExtensionViewModel(PyRevitExtensionDefinition extension) {
            var viewModel = new PyRevitExtensionViewModel(extension) { AllowChangeEnabled = true };
            viewModel.ToggleExtension = new DisableExtensionBehav(viewModel);

            return viewModel;
        }
    }
}
