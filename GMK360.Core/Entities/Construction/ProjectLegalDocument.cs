using System;

namespace GMK360.Core.Entities.Construction
{
    public enum LegalDocumentStatus
    {
        NotApplied = 0,    // BaYvurulmad / Yok
        Applied = 1,       // BaYvuruldu / YanKt Bekleniyor
        Issue = 2,         // Sorun KTktK / Reddedildi
        Approved = 3       // OnaylandK / Var
    }

    public class ProjectLegalDocument : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public int? SystemTemplateId { get; set; }
        public SystemLegalDocumentTemplate SystemTemplate { get; set; }

        public string DocumentName { get; set; }
        public string? Stage { get; set; }
        
        public LegalDocumentStatus Status { get; set; } = LegalDocumentStatus.NotApplied;
        public string? Notes { get; set; }
        
        public string? TrackingPerson { get; set; } 
        public string? AppliedTo { get; set; } 
        
        // Yeniden adlandiriyoruz / Amacini degistiriyoruz
        public string? InstitutionPhone { get; set; } // Istihbarat RFQ asamasina tasinacak ama veritabaninda kalabilir
        public string? InstitutionContact { get; set; } // YENI: Kurum Ici Ilgili / Tanidik / Memur Adi
        
        public DateTime? ExpiryDate { get; set; }
                public DateTime? ApplicationDate { get; set; } // Başvuru Tarihi
        public DateTime? AcquiredDate { get; set; } // Alındığı Tarih
        public string? FilePath { get; set; } // Yüklenen PDF/Görsel dosya yolu
        public decimal? DocumentCost { get; set; } // Harç / Masraf Tutarı
        
        public bool IsCustom { get; set; } // Globalde olmayan, firmanin kendi ekledigi evrak mi?
    }
}

