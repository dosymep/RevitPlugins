using System.Windows;
using System.Windows.Input;

namespace RevitPylonDocumentation.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitPylonDocumentation);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void SelectAllHostMarks(object sender, RoutedEventArgs e) {
            hostMarks.SelectAll();
        }
        private void UnselectAllHostMarks(object sender, RoutedEventArgs e) {
            hostMarks.UnselectAll();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            expander.MaxHeight = window.ActualHeight * 0.83;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if(e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }
    }
}
