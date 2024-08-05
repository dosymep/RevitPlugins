using System.Windows.Media;

using Color = Autodesk.Revit.DB.Color;

namespace RevitPylonDocumentation.Models {
    internal class ColorHelper {
        public ColorHelper(byte r, byte g, byte b) {
            UserColor = new Color(r, g, b);
            UserBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
        }

        public Color UserColor { get; set; }
        public SolidColorBrush UserBrush { get; set; }
    }
}
