using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class RevitProject {
        private readonly RevitDocumentViewModel _document;
        private readonly IList<RoomElement> _rooms;
        private readonly IList<Apartment> _apartments;
        private readonly DeclarationSettings _settings;
        private readonly RevitRepository _revitRepository;
        private readonly Phase _phase;

        public RevitProject(RevitDocumentViewModel document,
                            RevitRepository revitRepository,
                            DeclarationSettings settings) {
            _document = document;
            _settings = settings;
            _revitRepository = revitRepository;

            _phase = revitRepository.GetPhaseByName(document.Document, _settings.SelectedPhase.Name);

            _rooms = revitRepository.GetRoomsOnPhase(document.Document, _phase, settings);
            _rooms = FilterApartmentRooms(_rooms);
            _apartments = revitRepository.GetApartments(_rooms, settings);
        }

        /// <summary>Rooms on selected phase than belong to apartments</summary>
        public IList<RoomElement> Rooms => _rooms;
        public IList<Apartment> Apartments => _apartments;
        public Phase Phase => _phase;

        private IList<RoomElement> FilterApartmentRooms(IList<RoomElement> rooms) {
            return rooms
                .Where(x => x.GetTextParamValue(_settings.FilterRoomsParam) == _settings.FilterRoomsValue)
                .ToList();
        }

        public ErrorsListViewModel CheckApartmentsInRpoject() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Ошибка",
                Description = "В проекте отсутствуют квартиры на выбранной стадии",
                DocumentName = _document.Name
            };

            if(_apartments.Count == 0) {
                errorListVM.Errors = new List<ErrorElement>() {
                    new ErrorElement(_settings.SelectedPhase.Name, "Отсутствуют помещения квартир")
                };
            }

            return errorListVM;
        }

        public ErrorsListViewModel CheckRoomAreasEquality() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Ошибка",
                Description = "Площади, рассчитанные квартирографией отличаются в пределах квартиры",
                DocumentName = _document.Name
            };

            foreach(var apartment in _apartments) {
                if(!apartment.CheckEqaulityOfRoomAreas()) {
                    string apartInfo = $"Квартира № {apartment.Number} на этаже {apartment.Level}";
                    string apartAreas = "Площади квартиры (без коэффициента/с коэффициентом/жилая) " +
                        "должны быть одинаковыми для каждого помещения квартиры";
                    errorListVM.Errors.Add(new ErrorElement(apartInfo, apartAreas));
                }
            }

            return errorListVM;
        }

        public ErrorsListViewModel CheckActualRoomAreas() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Предупреждение",
                Description = "Не актуальные площади помещений, рассчитанные квартирографией",
                DocumentName = _document.Name
            };

            foreach(var apartment in _apartments) {
                if(!apartment.CheclActualRoomAreas()) {
                    string apartInfo = $"Квартира № {apartment.Number} на этаже {apartment.Level}";
                    string apartAreas = "Площади помещений, рассчитанные квартирографией " +
                        "отличаются от актуальной системной площадей помещения.";
                    errorListVM.Errors.Add(new ErrorElement(apartInfo, apartAreas));
                }
            }

            return errorListVM;
        }

        public ErrorsListViewModel CheckActualApartmentAreas() {
            ErrorsListViewModel errorListVM = new ErrorsListViewModel() {
                Message = "Предупреждение",
                Description = "Не актуальные площади квартир, рассчитанные квартирографией",
                DocumentName = _document.Name
            };

            foreach(var apartment in _apartments) {
                if(!apartment.CheclActualApartmentAreas()) {
                    string apartInfo = $"Квартира № {apartment.Number} на этаже {apartment.Level}";
                    string apartAreas = "Площади квартиры, рассчитанные квартирографией " +
                        "отличаются от суммы актуальных системных площадей этой квартиры. " +
                        "Проверьте общую площадь квартиры, площадь с коэффициентом, " +
                        "площадь жилых помещений и площадь без летних помещений";
                    errorListVM.Errors.Add(new ErrorElement(apartInfo, apartAreas));
                }
            }

            return errorListVM;
        }

        public void CalculateUtpForApartments() {
            UtpCalculator calculator = new UtpCalculator(this, _settings);

            foreach(var apartment in _apartments) {
                apartment.CalculateUtp(calculator);
            }
        }

        public List<FamilyInstance> GetDoors() {
            return _revitRepository
                .GetDoorsOnPhase(_document.Document, _phase)
                .ToList();
        }

        public List<FamilyInstance> GetBathInstances() {
            return _revitRepository.
                GetBathInstancesOnPhase(_document.Document, _phase)
                .ToList();
        }
    }
}
