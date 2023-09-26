﻿using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в стене
    /// </summary>
    internal class SingleRectangleOpeningTaskInWallParameterGetter : IParametersGetter {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в стене
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        public SingleRectangleOpeningTaskInWallParameterGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _openingMepTaskIncoming = incomingTask;
            _pointFinder = pointFinder;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningHeight, new RectangleOpeningTaskInWallHeightValueGetter(_openingMepTaskIncoming, _pointFinder)).GetParamValue();
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningWidth, new RectangleOpeningTaskWidthValueGetter(_openingMepTaskIncoming)).GetParamValue();

            // логические флаги для обозначений разделов отверстия
            var isEomValueGetter = new IsEomValueGetter(_openingMepTaskIncoming);
            var isSsValueGetter = new IsSsValueGetter(_openingMepTaskIncoming);
            var isOvValueGetter = new IsOvValueGetter(_openingMepTaskIncoming);
            var isDuValueGetter = new IsDuValueGetter(_openingMepTaskIncoming);
            var isVkValueGetter = new IsVkValueGetter(_openingMepTaskIncoming);
            var isTsValueGetter = new IsTsValueGetter(_openingMepTaskIncoming);
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsEom, isEomValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsSs, isSsValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsOv, isOvValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsDu, isDuValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsVk, isVkValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsTs, isTsValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsManualBimModelPart, new IsManualBimModelPartValueGetter()).GetParamValue();

            // текстовые данные отверстия
            var manualBimModelPartValueGetter = new ManualBimModelPartValueGetter()
                .SetIsEom(isEomValueGetter)
                .SetIsSs(isSsValueGetter)
                .SetIsOv(isOvValueGetter)
                .SetIsDu(isDuValueGetter)
                .SetIsVk(isVkValueGetter)
                .SetIsTs(isTsValueGetter)
                ;
            yield return new StringParameterGetter(RealOpeningPlacer.RealOpeningTaskId, new TaskIdValueGetter(_openingMepTaskIncoming)).GetParamValue();
            yield return new StringParameterGetter(RealOpeningPlacer.RealOpeningManualBimModelPart, manualBimModelPartValueGetter).GetParamValue();
        }
    }
}
