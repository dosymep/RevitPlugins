using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using DevExpress.Mvvm.Native;
using DevExpress.Xpf.Bars;

using dosymep.SimpleServices;

namespace RevitPlatformSettings.Views {
    public sealed partial class OpenSourceWindow {
        public OpenSourceWindow(ILocalizationService localizationService) {
            LocalizationService = localizationService;

            string siteType = localizationService.GetLocalizedString("OpenSourceWindow.SiteType");
            string toolType = localizationService.GetLocalizedString("OpenSourceWindow.ToolType");
            string libraryType = localizationService.GetLocalizedString("OpenSourceWindow.LibraryType");

            string freeLicense = localizationService.GetLocalizedString("OpenSourceWindow.FreeLicense");
            string communityLicense = localizationService.GetLocalizedString("OpenSourceWindow.CommunityLicense");

            string mitLicense = localizationService.GetLocalizedString("OpenSourceWindow.MITLicense");
            string gnuLicense = localizationService.GetLocalizedString("OpenSourceWindow.GNULicense");
            string apacheLicense = localizationService.GetLocalizedString("OpenSourceWindow.ApacheLicense");
            string riderLicense = localizationService.GetLocalizedString("OpenSourceWindow.RiderLicense");
            string devExpressLicense = localizationService.GetLocalizedString("OpenSourceWindow.DevExpressLicense");
            string openSourceLicense = localizationService.GetLocalizedString("OpenSourceWindow.OpenSourceLicense");
            string onlineLicense = localizationService.GetLocalizedString("OpenSourceWindow.OnlineDocumentation");

            var items = new List<OpenSourceItem>() {
                new OpenSourceItem() {
                    ItemType = siteType,
                    AuthorName = "Icons8",
                    AuthorNavigationUrl = "https://icons8.com",
                    SoftwareName = "Icons8",
                    SoftwareNavigationUrl = "https://icons8.com",
                    LicenseName = freeLicense,
                    LicenseNavigationUrl = "https://icons8.com/license"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "JetBrains",
                    AuthorNavigationUrl = "https://www.jetbrains.com",
                    SoftwareName = "JetBrains Rider",
                    SoftwareNavigationUrl = "https://www.jetbrains.com/rider",
                    LicenseName = riderLicense,
                    LicenseNavigationUrl = "https://www.jetbrains.com/rider"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "JetBrains",
                    AuthorNavigationUrl = "https://www.jetbrains.com",
                    SoftwareName = "JetBrains pyCharm",
                    SoftwareNavigationUrl = "https://www.jetbrains.com/pycharm",
                    LicenseName = communityLicense,
                    LicenseNavigationUrl = "https://www.jetbrains.com/pycharm"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "Microsoft",
                    AuthorNavigationUrl = "https://microsoft.com",
                    SoftwareName = "MS Visual Studio",
                    SoftwareNavigationUrl = "https://visualstudio.microsoft.com/vs",
                    LicenseName = communityLicense,
                    LicenseNavigationUrl = "https://visualstudio.microsoft.com/vs"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "PVS-Studio",
                    AuthorNavigationUrl = "https://pvs-studio.com",
                    SoftwareName = "PVS-Studio",
                    SoftwareNavigationUrl = "https://pvs-studio.com/en/pvs-studio",
                    LicenseName = openSourceLicense,
                    LicenseNavigationUrl = "https://pvs-studio.com/en/order/open-source-license/"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "nuke-build",
                    AuthorNavigationUrl = "https://nuke.build",
                    SoftwareName = "Nuke",
                    SoftwareNavigationUrl = "https://github.com/nuke-build/nuke",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/nuke-build/nuke/blob/develop/LICENSE"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "dosymep.Revit",
                    SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.Revit",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/dosymep/dosymep.Revit/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "dosymep.Autodesk",
                    SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.Autodesk",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/dosymep/dosymep.Autodesk/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "dosymep.SimpleServices",
                    SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.SimpleServices",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/dosymep/dosymep.SimpleServices/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "Serilog.Enrichers.Autodesk.Revit",
                    SoftwareNavigationUrl = "https://github.com/dosymep/Serilog.Enrichers.Autodesk.Revit",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl =
                        "https://github.com/dosymep/Serilog.Enrichers.Autodesk.Revit/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "Serilog.Sinks.Autodesk.Revit",
                    SoftwareNavigationUrl = "https://github.com/dosymep/Serilog.Sinks.Autodesk.Revit",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl =
                        "https://github.com/dosymep/Serilog.Sinks.Autodesk.Revit/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "Autodesk.Revit.Sdk.Refs",
                    SoftwareNavigationUrl = "https://github.com/dosymep/Autodesk.Revit.Sdk.Refs",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/dosymep/Autodesk.Revit.Sdk.Refs/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dosymep",
                    AuthorNavigationUrl = "https://dosymep.net/",
                    SoftwareName = "dosymep.Nuke.RevitVersions",
                    SoftwareNavigationUrl = "https://github.com/dosymep/dosymep.Nuke.RevitVersions",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl =
                        "https://github.com/dosymep/dosymep.Nuke.RevitVersions/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "JamesNK",
                    AuthorNavigationUrl = "https://github.com/JamesNK",
                    SoftwareName = "Newtonsoft.Json",
                    SoftwareNavigationUrl = "https://github.com/JamesNK/Newtonsoft.Json",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "dahall",
                    AuthorNavigationUrl = "https://github.com/dahall",
                    SoftwareName = "vanara",
                    SoftwareNavigationUrl = "https://github.com/dahall/vanara",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/dahall/Vanara/blob/master/LICENSE"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "serilog",
                    AuthorNavigationUrl = "https://github.com/serilog",
                    SoftwareName = "serilog",
                    SoftwareNavigationUrl = "https://github.com/serilog/serilog",
                    LicenseName = apacheLicense,
                    LicenseNavigationUrl = "https://github.com/serilog/serilog/blob/dev/LICENSE"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "ninject",
                    AuthorNavigationUrl = "https://github.com/ninject",
                    SoftwareName = "Ninject",
                    SoftwareNavigationUrl = "https://github.com/ninject/Ninject",
                    LicenseName = apacheLicense,
                    LicenseNavigationUrl = "https://github.com/ninject/Ninject/blob/main/LICENSE.txt"
                },
                new OpenSourceItem() {
                    ItemType = libraryType,
                    AuthorName = "DevExpress",
                    AuthorNavigationUrl = "https://www.devexpress.com",
                    SoftwareName = "DevExpress",
                    SoftwareNavigationUrl = "https://www.devexpress.com",
                    LicenseName = devExpressLicense,
                    LicenseNavigationUrl = "https://www.devexpress.com"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "jeremytammik",
                    AuthorNavigationUrl = "https://github.com/jeremytammik",
                    SoftwareName = "RevitLookup",
                    SoftwareNavigationUrl = "https://github.com/jeremytammik/RevitLookup",
                    LicenseName = mitLicense,
                    LicenseNavigationUrl = "https://github.com/jeremytammik/RevitLookup/blob/dev/License.md"
                },
                new OpenSourceItem() {
                    ItemType = siteType,
                    AuthorName = "Gui Talarico",
                    AuthorNavigationUrl = "https://gtalarico.com",
                    SoftwareName = "revitapidocs",
                    SoftwareNavigationUrl = "https://www.revitapidocs.com",
                    LicenseName = onlineLicense,
                    LicenseNavigationUrl = "https://www.revitapidocs.com"
                },
                new OpenSourceItem() {
                    ItemType = toolType,
                    AuthorName = "pyrevitlabs",
                    AuthorNavigationUrl = "https://github.com/pyrevitlabs",
                    SoftwareName = "pyRevit",
                    SoftwareNavigationUrl = "https://github.com/pyrevitlabs/pyRevit",
                    LicenseName = gnuLicense,
                    LicenseNavigationUrl = "https://github.com/pyrevitlabs/pyRevit/blob/master/LICENSE.txt"
                },
            };

            Items = items
                .OrderBy(item => item.ItemType)
                .ToObservableCollection();

            InitializeComponent();
        }

        public override string PluginName => nameof(RevitPlatformSettings);
        public override string ProjectConfigName => nameof(OpenSourceWindow);

        public ObservableCollection<OpenSourceItem> Items { get; }
    }

    public class OpenSourceItem {
        public string ItemType { get; set; }
        
        public string AuthorName { get; set; }
        public string AuthorNavigationUrl { get; set; }

        public string SoftwareName { get; set; }
        public string SoftwareNavigationUrl { get; set; }

        public string LicenseName { get; set; }
        public string LicenseNavigationUrl { get; set; }
    }
}
