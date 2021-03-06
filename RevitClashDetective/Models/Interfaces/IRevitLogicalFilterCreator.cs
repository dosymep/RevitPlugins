using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    interface IRevitLogicalFilterCreator {
        ElementLogicalFilter Create(IList<ElementFilter> elementFilters);
    }
}
