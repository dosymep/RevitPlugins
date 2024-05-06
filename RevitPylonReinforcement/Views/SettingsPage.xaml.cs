using System;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.UI;

namespace RevitPylonReinforcement.Views {
    /// <summary>
    /// Логика взаимодействия для SettingsMainPage.xaml
    /// </summary>
    public partial class SettingsPage : Page, IDisposable, IDockablePaneProvider {
        #region constructor

        public SettingsPage() {
            InitializeComponent();
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
            data.EditorInteraction = new EditorInteraction(EditorInteractionType.KeepAlive);

            data.InitialState = new DockablePaneState {

                TabBehind = DockablePanes.BuiltInDockablePanes.PropertiesPalette,
                DockPosition = DockPosition.Tabbed
            };
        }

        #endregion
    }
}
