using System;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.UI;

namespace RevitPylonReinforcement.Views {
    /// <summary>
    /// Логика взаимодействия для SettingsMainPage.xaml
    /// </summary>
    public partial class SettingsMainPage : Page, IDisposable, IDockablePaneProvider {
        #region constructor

        /// <summary>
        /// Default contructor.
        /// Initializes a new instance of the <see cref="FamilyManagerMainPage"/> class.
        /// </summary>
        public SettingsMainPage() {
            InitializeComponent();

            // Set data context for main application page.
            //DataContext = new FamilyManagerMainPageViewModel();
        }


        #endregion

        #region public methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            this.Dispose();
        }

        /// <summary>
        /// Setups the dockable pane.
        /// </summary>
        public void SetupDockablePane(DockablePaneProviderData data) {
            data.FrameworkElement = this as FrameworkElement;

            // Define initial pane position in Revit ui.
            data.InitialState = new DockablePaneState {
                DockPosition = DockPosition.Right,
            };
        }

        #endregion
    }
}
