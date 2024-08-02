using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RevitPlatformSettings.Views {
    [DefaultProperty(nameof(EditControl))]
    [System.Windows.Markup.ContentProperty(nameof(EditControl))]
    public partial class CustomEditControl {
        public static readonly DependencyProperty EditTextProperty = DependencyProperty.Register(
            nameof(EditText), typeof(string), typeof(CustomEditControl), new PropertyMetadata(default(string)));


        public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
            nameof(EditControl), typeof(object), typeof(CustomEditControl), new PropertyMetadata(default(object)));

        public CustomEditControl() {
            InitializeComponent();
        }

        public string EditText {
            get => (string) GetValue(EditTextProperty);
            set => SetValue(EditTextProperty, value);
        }

        public object EditControl {
            get => (object) GetValue(EditControlProperty);
            set => SetValue(EditControlProperty, value);
        }
    }
}

