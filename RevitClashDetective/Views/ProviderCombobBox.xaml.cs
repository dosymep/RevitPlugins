using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using RevitClashDetective.ViewModels;

namespace RevitClashDetective.Views {
    /// <summary>
    /// Interaction logic for ProviderCombobBox.xaml
    /// </summary>
    public partial class ProviderCombobBox : UserControl {

        public ProviderCombobBox() {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox comboBox = (ComboBox) sender;
            comboBox.SelectedItem = null;
        }
    }
}
