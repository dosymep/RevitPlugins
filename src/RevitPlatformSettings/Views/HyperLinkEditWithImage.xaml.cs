using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RevitPlatformSettings.Views {
    public partial class HyperLinkEditWithImage {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource), typeof(ImageSource), typeof(HyperLinkEditWithImage),
            new PropertyMetadata(default(ImageSource)));

        public static readonly DependencyProperty EditValueProperty = DependencyProperty.Register(
            nameof(EditValue), typeof(object), typeof(HyperLinkEditWithImage), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty NavigationUrlProperty = DependencyProperty.Register(
            nameof(NavigationUrl), typeof(string), typeof(HyperLinkEditWithImage),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(ICommand), typeof(HyperLinkEditWithImage), new PropertyMetadata(default(ICommand)));

        public ICommand Command {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public string NavigationUrl {
            get => (string) GetValue(NavigationUrlProperty);
            set => SetValue(NavigationUrlProperty, value);
        }

        public object EditValue {
            get => (object) GetValue(EditValueProperty);
            set => SetValue(EditValueProperty, value);
        }

        public ImageSource ImageSource {
            get => (ImageSource) GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public HyperLinkEditWithImage() {
            InitializeComponent();
        }
    }
}
