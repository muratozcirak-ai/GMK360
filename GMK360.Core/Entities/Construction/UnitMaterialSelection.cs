using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Construction
{
    public class UnitMaterialSelection : BaseEntity
    {
        public int BuildingUnitId { get; set; }
        public BuildingUnit BuildingUnit { get; set; }

        public int? ProjectMaterialCatalogId { get; set; }
        public ProjectMaterialCatalog? ProjectMaterialCatalog { get; set; }

        public string? Category { get; set; } // Örn: Salon & Odalar Zemin (Laminant Parke)

        public string? SelectedByUserId { get; set; } // Seçimi yapan kat sahibi
        public ApplicationUser? SelectedByUser { get; set; }

        public DateTime SelectionDate { get; set; } = DateTime.Now;
        public bool IsApprovedByAdmin { get; set; } = false; // Firma onayladı mı?

        // Özel İstek & Fotoğraf Yükleme
        public string? CustomRequestImageUrl { get; set; }
        public string? CustomRequestNotes { get; set; }
        public decimal? ApprovedPriceDifference { get; set; }

        // Anlaşmalı Showroom / Tedarikçi Ziyareti
        public string? ShowroomSupplierName { get; set; }
        public string? ShowroomProductCode { get; set; }

        // SMS OTP Güvencesi & Tarih/Saat (Anayasa Madde 8)
        public string? VerificationPhone { get; set; }
        public string? OtpCode { get; set; }
        public bool IsConfirmedViaSms { get; set; } = false;
        public DateTime? ConfirmedDate { get; set; }
        public TimeSpan? ConfirmedTime { get; set; }
        public string? SelectionStatus { get; set; } // Taslak, SmsBekliyor, MusteriOnayli, FiyatBekliyor, ImalataGonderildi
    }
}
