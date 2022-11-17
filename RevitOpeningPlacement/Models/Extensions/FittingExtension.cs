﻿using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class FittingExtension {
        public static double GetMaxConnectorArea(this FamilyInstance fitting) {
            var connectors = fitting.GetConnectors();
            if(connectors == null) {
                return 0;
            }
            return connectors.Max(item => item.GetArea());
        }

        public static bool IsHorizontal(this FamilyInstance fitting) {
            var connectors = fitting.GetConnectors();
            if(connectors == null) {
                return false;
            }
            var connectorHeights = connectors.Select(item => item.CoordinateSystem.Origin.Z).ToArray();
            return Math.Abs(connectorHeights.Average() - connectorHeights.First()) < 0.0001;
        }

        public static double GetMaxDiameter(this FamilyInstance fitting) {
            var connectors = fitting.GetConnectors();
            if(connectors == null) {
                return 0;
            }
            return connectors.Max(item => item.GetDiameter());
        }

        public static double GetMaxHeight(this FamilyInstance fitting) {
            var connectors = fitting.GetConnectors();
            if(connectors == null) {
                return 0;
            }
            return connectors.Max(item => item.GetHeight());
        }

        public static double GetMaxWidth(this FamilyInstance fitting) {
            var connectors = fitting.GetConnectors();
            if(connectors == null) {
                return 0;
            }
            return connectors.Max(item => item.GetWidth());
        }

        public static bool HasConnectors(this FamilyInstance fitting) {
            return fitting.MEPModel != null
                && fitting.MEPModel.ConnectorManager != null
                && fitting.MEPModel.ConnectorManager.Connectors != null
                && fitting.MEPModel.ConnectorManager.Connectors.OfType<Connector>().Any();
        }

        public static List<Connector> GetConnectors(this FamilyInstance fitting) {
            if(!fitting.HasConnectors()) {
                return null;
            }
            return fitting.MEPModel.ConnectorManager.Connectors
                .OfType<Connector>()
                .ToList();
        }
    }
}
