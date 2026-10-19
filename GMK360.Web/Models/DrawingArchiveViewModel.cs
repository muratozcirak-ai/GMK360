using System;
using System.Collections.Generic;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;

namespace GMK360.Web.Models
{
    public class DrawingArchiveViewModel
    {
        public ConstructionProject Project { get; set; }
        public List<Building> Blocks { get; set; } = new List<Building>();
        public List<FloorPlanMatrixItem> FloorMatrix { get; set; } = new List<FloorPlanMatrixItem>();
        public List<DocumentArchive> AllDrawings { get; set; } = new List<DocumentArchive>();
        public List<DocumentArchive> ArchitecturalDrawings { get; set; } = new List<DocumentArchive>();
        public List<DocumentArchive> StructuralDrawings { get; set; } = new List<DocumentArchive>();
        public List<DocumentArchive> MechanicalDrawings { get; set; } = new List<DocumentArchive>();
        public List<DocumentArchive> ElectricalDrawings { get; set; } = new List<DocumentArchive>();

        // İstatistikler
        public int TotalFloorsCount { get; set; }
        public int FloorsWithPlanCount { get; set; }
        public int FloorsMissingPlanCount => Math.Max(0, TotalFloorsCount - FloorsWithPlanCount);
        public double PlanCompletionPercentage => TotalFloorsCount > 0 ? Math.Round((double)FloorsWithPlanCount / TotalFloorsCount * 100, 1) : 0;
    }

    public class FloorPlanMatrixItem
    {
        public int BuildingId { get; set; }
        public string BlockName { get; set; }
        public bool IsChildBuilding { get; set; }
        public int? ParentBuildingId { get; set; }
        public int FloorLevel { get; set; }
        public string FloorName { get; set; }
        public int UnitCount { get; set; }
        public double TotalGrossM2 { get; set; }
        public double TotalNetM2 { get; set; }
        public bool HasPlan { get; set; }
        public string? PlanUrl { get; set; }
        public string? PlanFileName { get; set; }
        public string? PlanExtension { get; set; }
        public DateTime? PlanUploadDate { get; set; }
        public long? PlanSizeBytes { get; set; }
    }
}
