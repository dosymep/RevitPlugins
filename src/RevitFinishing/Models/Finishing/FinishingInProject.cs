using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models
{
    /// <summary>
    /// Класс для хранения всех семейств отделки в текущем проекте Revit.
    /// Семейства отфильтрованы с учетом стадии, выбранной пользователем.
    /// Семейства сгруппированы по категориям отделки.
    /// </summary>
    internal class FinishingInProject {
        private readonly IReadOnlyCollection<Element> _walls;
        private readonly IReadOnlyCollection<Element> _floors;
        private readonly IReadOnlyCollection<Element> _ceilings;
        private readonly IReadOnlyCollection<Element> _baseboards;
        private readonly IReadOnlyCollection<Element> _allFinishing;

        public FinishingInProject(RevitRepository revitRepository, Phase phase) {
            _walls = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Walls, phase).ToList();
            _floors = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Floors, phase).ToList();
            _ceilings = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Ceilings, phase).ToList();
            _baseboards = revitRepository.GetFinishingElementsOnPhase(FinishingCategory.Baseboards, phase).ToList();

            _allFinishing = _walls
                .Concat(_baseboards)
                .Concat(_ceilings)
                .Concat(_floors)
                .ToList();
        }
        public IReadOnlyCollection<Element> Walls => _walls;
        public IReadOnlyCollection<Element> Floors => _floors;
        public IReadOnlyCollection<Element> Ceilings => _ceilings;
        public IReadOnlyCollection<Element> Baseboards => _baseboards;
        public IReadOnlyCollection<Element> AllFinishing => _allFinishing;
    }
}
