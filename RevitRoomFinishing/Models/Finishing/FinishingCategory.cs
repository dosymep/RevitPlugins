using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitRoomFinishing.Models {
    internal class FinishingCategory {
        public string Name { get; set; }
        public string KeyWord { get; set; }
        public BuiltInCategory Category { get; set; }

        public static FinishingCategory Walls { get; } = new FinishingCategory() {
            Name = "Стены",
            KeyWord = "(О) Стена",
            Category = BuiltInCategory.OST_Walls
        };

        public static FinishingCategory Baseboards { get; } = new FinishingCategory() {
            Name = "Плинтусы",
            KeyWord = "(О) Плинтус",
            Category = BuiltInCategory.OST_Walls
        };

        public static FinishingCategory Floors { get; } = new FinishingCategory() {
            Name = "Перекрытия",
            KeyWord = "(АР)",
            Category = BuiltInCategory.OST_Floors
        };

        public static FinishingCategory Ceilings { get; } = new FinishingCategory() {
            Name = "Потолки",
            KeyWord = "(О) Потолок",
            Category = BuiltInCategory.OST_Ceilings
        };

    }
}
