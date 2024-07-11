using System.Windows;
using System.Windows.Controls;

namespace RevitFinishing.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitFinishing);
        public override string ProjectConfigName => nameof(MainWindow);
        
        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ChangeSelected();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            ChangeSelected();
        }

        private void ChangeSelected() {
            var listBox = (ListBox) FindName("RoomGroups");
            var groups = listBox.SelectedItems;
            //foreach(RoomGroupViewModel group in groups) {
            //    group.IsChecked = state;
            //}
        }
    }
}
