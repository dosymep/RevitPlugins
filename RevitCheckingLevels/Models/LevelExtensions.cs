﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCheckingLevels.Models {
    internal static class LevelExtensions {
        public static readonly Units MetricUnits 
            = new Units(UnitSystem.Metric) { DecimalSymbol = DecimalSymbol.Dot };

        public static bool IsAlmostEqual(double left, double right,
            double eps = double.Epsilon) {
            return Math.Abs(left - right) < eps;
        }

#if REVIT_2020_OR_LESS

        public static readonly FormatOptions FormatOptions 
            = new FormatOptions(DisplayUnitType.DUT_METERS) { Accuracy = 0.001 };

        public static string GetFormattedMeterElevation(Level level) {
            var formatValueOptions = new FormatValueOptions();
            formatValueOptions.SetFormatOptions(FormatOptions);
            formatValueOptions.AppendUnitSymbol = true;
            return UnitFormatUtils.Format(MetricUnits, UnitType.UT_Length,
                level.Elevation, false, false, formatValueOptions);
        }

        public static double GetMeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);

        public static double GetMillimeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_MILLIMETERS);

#else
        public static readonly FormatOptions FormatOptions 
            = new FormatOptions(UnitTypeId.Meters) { Accuracy = 0.001 };

        public static string GetFormattedMeterElevation(Level level) {
            var formatValueOptions = new FormatValueOptions();
            formatValueOptions.SetFormatOptions(FormatOptions);
            formatValueOptions.AppendUnitSymbol = true;
            return UnitFormatUtils.Format(MetricUnits, SpecTypeId.Length,
                level.Elevation, false, formatValueOptions);
        }

        public static double GetMeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);

        public static double GetMillimeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Millimeters);

#endif
    }
}
