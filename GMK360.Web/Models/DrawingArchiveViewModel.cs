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

        // Toptan İhale & Satın Alma Masası (Master BOM)
        public List<MasterBomItemDto> MasterBomItems { get; set; } = new List<MasterBomItemDto>();

        // Şantiye Saha Karar & Donanım Tercihleri
        public FieldDecisionsDto Decisions { get; set; } = new FieldDecisionsDto();

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

    public class MasterBomItemDto
    {
        public int Id { get; set; }
        public string Discipline { get; set; } = ""; // Elektrik, Mekanik, Statik, İksa, Mimari
        public string Category { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Formula { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = ""; // Metre, Adet, Ton, m², m³, Takım, Torba
        public string ProfessionName { get; set; } = ""; // Elektrikçi, Tesisatçı, Demirci, Betoncu, Doğramacı vb.
        public string SourceDwg { get; set; } = ""; // Çizim referansı
        public string RuleNote { get; set; } = ""; // Z ekseni payı, vb.
        public string BadgeColor { get; set; } = "primary";

        // Tedarikçi / Taşeron İhale Paketi Alanları
        public string PackageKey { get; set; } = ""; // örn: pkg_demir, pkg_beton, pkg_ahsap
        public string PackageName { get; set; } = ""; // örn: Parke & Orman Ürünleri Mağazası
        public string PackageIcon { get; set; } = "bi-box"; // Bootstrap ikon adı
        public int PackageOrder { get; set; } = 1; // Sıralama önceliği

        // Saha Karar & Tetikleyici Alanları
        public bool IsInjected { get; set; } = false;
        public string? TriggerKey { get; set; }
        public string? TriggerOption { get; set; }

        // 9 Kolonlu Malzeme Tedarik & Satın Alma Matrisi Alanları (Liste A: Ham Malzemeler)
        public string SupplierBranch { get; set; } = ""; // B: Tedarikçi Türü
        public decimal? UnitPrice { get; set; } // F: Birim Fiyat
        public decimal? TotalPrice => UnitPrice.HasValue ? Math.Round(Quantity * UnitPrice.Value, 2) : null; // G: Toplam (Miktar x Fiyat)
        public bool IsSeparateResearch { get; set; } = false; // H: Ayrı Araştır (Spot / Parti Sonu)
        public string? AgreedSupplier { get; set; } = ""; // I: Anlaşılan Yer / Tedarikçi Firma
        public decimal? LastPurchasedPrice { get; set; } // En son alınan işten referans birim fiyat
    }

    public class FieldDecisionsDto
    {
        public string KuleVinc { get; set; } = "Kiralik"; // Kiralik, Yok
        public string PerdeYalitimi { get; set; } = "MembranXPS"; // MembranXPS, Surme, Yok
        public string SogukDerzDrenaj { get; set; } = "Var"; // Var, Yok
        public string IhataTipi { get; set; } = "TrapezSac"; // TrapezSac, TelCit, Yok
        public string Mobilizasyon { get; set; } = "OfisWc"; // OfisWc, Kamp, Yok
        public string Vestiyer { get; set; } = "Var"; // Var, Yok
        public string Panjur { get; set; } = "Var"; // Var, Yok
        public string Jenerator { get; set; } = "OrtakAlan"; // OrtakAlan, TamBesleme, Yok
        public string EvSarj { get; set; } = "Var"; // Var, Yok
        public string SosyalDonati { get; set; } = "Full"; // Full, Kamelya, Yok
    }
}
