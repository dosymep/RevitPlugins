using System.Windows;
using System.Windows.Controls;

using Revit3DvikSchemas.ViewModels;

namespace Revit3DvikSchemas.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(Revit3DvikSchemas);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ChangeSelected(true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            ChangeSelected(false);
        }

        private void ChangeSelected(bool state) {
            var listView = (ListView) FindName("HvacSystems");
            var hvacSystems = listView.SelectedItems;
            foreach(HvacSystemViewModel hvacSystem in hvacSystems) {
                hvacSystem.IsChecked = state;
            }
        }
    }
}