﻿using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal abstract class Clash<T1, T2> where T1 : Element
                                 where T2 : Element {
        public Clash(RevitRepository revitRepository, ClashModel clashModel) {
            bool exceptionWasThrown = false;
            try {
                Element1 = (T1) clashModel.MainElement.GetElement(revitRepository.DocInfos);
            } catch(InvalidCastException) {
                // Дальше этот Clash попадет в Unplaced
                exceptionWasThrown = true;
            }
            try {
                Element2 = (T2) clashModel.OtherElement.GetElement(revitRepository.DocInfos);
            } catch(InvalidCastException) {
                // Дальше этот Clash попадет в Unplaced
                exceptionWasThrown = true;
            }
            if(!exceptionWasThrown) {
                Element2Transform = revitRepository.GetTransform(Element2);
            }
        }

        public T1 Element1 { get; set; }
        public T2 Element2 { get; set; }
        public Transform Element2Transform { get; set; }

        public Solid GetIntersection() {
            return Element1.GetSolid().GetIntersection(Element2.GetSolid(), Element2Transform);
        }

        public abstract double GetConnectorArea();
    }
}
