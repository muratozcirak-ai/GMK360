using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class ProjectTask : BaseEntity
    {
        public int ProjectId { get; set; }

        public int? CustomerUnitId { get; set; } // Görev tüm kata değil de daireye aitse burası dolar.

        public ConstructionProject Project { get; set; }

        public int? ElevationId { get; set; } // Bu iş hangi katta yapılıyor?
        public ProjectElevation Elevation { get; set; }
        
        // Temel Görev Bilgileri
        public string TaskName { get; set; }
        public string? WorkGroupCategory { get; set; } // Şablondan kopya

        // DXF ve Maliyet Köprüsü
        
        // Lojistik ve Malzeme Kilidi
        public bool IsMaterialReady { get; set; } 
        public string? MaterialShortageNote { get; set; } // Örn: "16'lık demir kamyonu gecikti"
        public DateTime? PlannedStartDate { get; set; }

        public string? WorkItemCode { get; set; } // Hangi reçeteye bağlı? ("DUVAR_YTONG_20")
        public decimal TotalQuantity { get; set; }

        public decimal CompletedQuantity { get; set; } // Başlangıçta 0'dır. Usta çalıştıkça artar. Toplam 500'e ulaşınca iş "Bitti" olur.
 // DXF'ten gelen net metraj

        public string WorkflowType { get; set; } // Kaba, InceSabit, InceSecmeli
        public string Status { get; set; } // "Bekliyor", "Aktif", "OnayBekliyor", "Bitti", "REDDEDILDI"
        
        // Esneklik ve Görünmez Maliyet (Ekstra İşler)
        public bool IsExtraWork { get; set; } // Şablonda olmayan, şefin anlık eklediği iş mi?
        public string CostCategory { get; set; } // Zarar tespiti için: "Standart", "Hata Düzeltme", "Hava Muhalefeti"
        public string CustomNote { get; set; } // Şefin o işe özel girdiği günlük notu
        
        // Dondurma Butonu ve Domino Etkisi Algoritması
        public bool IsPaused { get; set; } // Şef işi dondurdu mu?
        public string PauseReason { get; set; } // Örn: "Şiddetli Yağmur", "Belediye Mühürü"
        public int DelayDays { get; set; } // Takvimi ne kadar öteleyecek?
        
        // Taşeron Bağlantısı ve CRM (Muhasebe Köprüsü)
        public int? SubcontractorId { get; set; } // Bu işi hangi Taşeron yapıyor?
        public decimal ProgressPaymentAmount { get; set; } // Hakediş Tutarı
        public int? RequiredCustomerId { get; set; } // CRM'den beklenen onayın (müşterinin) ID'si
    }
}
