﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models {
    internal class CategoryElements {
        public CategoryElements(Category cat) {
            CategoryInView = cat;
        }

        public Category CategoryInView { get; set; }

        public List<Element> ElementsInView { get; set; } = new List<Element>();



    }
}
