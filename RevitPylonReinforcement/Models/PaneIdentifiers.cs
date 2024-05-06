using System;

namespace RevitPylonReinforcement.Models {
    /// <summary>
    /// Guid container that holds guid references to dockable panes.
    /// </summary>
    public static class PaneIdentifiers {
        #region public methods

        /// <summary>
        /// The family manager dockable pane identifier.
        /// </summary>
        /// <returns></returns>
        public static Guid GetManagerPaneIdentifier() {
            return new Guid("8019815A-EFC5-4C82-9462-338670C274C1");
        }

        #endregion
    }
}
