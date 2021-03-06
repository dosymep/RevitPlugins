using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using dosymep.WPF.Views;

namespace RevitServerFolders.Export {
    /// <summary>
    /// Interaction logic for ExportNwcFileWindow.xaml
    /// </summary>
    public partial class ExportNwcFileWindow : PlatformWindow {
        public ExportNwcFileWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitServerFolders);
        public override string ProjectConfigName => nameof(ExportNwcFileWindow);

        private void _btOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
