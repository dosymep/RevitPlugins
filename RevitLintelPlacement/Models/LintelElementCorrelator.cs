using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {
    internal class LintelElementCorrelator {
        public List<ICorrelator> Correlators { get; set; }
        
        public LintelElementCorrelator(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {

            Correlators = new List<ICorrelator> {
                new SuperComponentCorrelator(revitRepository),
                new DimensionCorrelator(revitRepository),
                new GeometricalCorrelator(revitRepository, elementInfos)
            };
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            return Correlators.Select(correlator => correlator.Correlate(lintel))
                .FirstOrDefault(element => element != null);
        }
    }

    internal class SuperComponentCorrelator : ICorrelator {
        private readonly RevitRepository _revitRepository;

        public SuperComponentCorrelator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            if(lintel.SuperComponent != null) {
                return (FamilyInstance) _revitRepository.GetElementById(lintel.SuperComponent.Id);
            }
            return null;
        }
    }

    internal class DimensionCorrelator : ICorrelator {
        private readonly RevitRepository _revitRepository;

        public DimensionCorrelator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            var alignElement = _revitRepository.GetDimensionFamilyInstance(lintel);
            if(alignElement == null) {
                return null;
            }

            if(alignElement.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id ||
               alignElement.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Windows).Id) {
                return (FamilyInstance) _revitRepository.GetElementById(alignElement.Id);
            }
            return null;
        }
    }

    internal class GeometricalCorrelator : ICorrelator {
        private readonly Dictionary<XYZ, FamilyInstance> _elementLocationDict;

        public GeometricalCorrelator(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            _elementLocationDict = revitRepository
                  .GetAllElementsInWall(SampleMode.AllElements, elementInfos)
                  .Where(e => e.Location != null)
                  .ToDictionary(revitRepository.GetLocationPoint);
        }

        public FamilyInstance Correlate(FamilyInstance lintel) {
            var lintelLocation = ((LocationPoint) lintel.Location).Point;
            var nearestPoint = _elementLocationDict.Keys.OrderBy(item => lintelLocation.DistanceTo(item)).FirstOrDefault();
            return nearestPoint != null
                ? lintelLocation.DistanceTo(nearestPoint) < 1.6 ? _elementLocationDict[nearestPoint] : null
                : null;
        }
    }
}
