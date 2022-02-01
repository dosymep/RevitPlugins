﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitLintelPlacement.Models.Interfaces {
    internal interface ICorrelator {
        FamilyInstance Correlate(RevitRepository revitRepository, FamilyInstance lintel, Dictionary<XYZ, FamilyInstance> elementLocationDict = null);
    }
}
