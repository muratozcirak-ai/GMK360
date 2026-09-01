using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class BuildingContract : BaseEntity
    {
        public int? HousingComplexId { get; set; }
        public virtual HousingComplex? HousingComplex { get; set; }

        public int? BuildingId { get; set; }
        public virtual Building? Building { get; set; }

        public string ContractType { get; set; } = null!; // "Güvenlik", "Temizlik", "Asansör Bakım", "Peyzaj"
        
        public string Title { get; set; } = null!; // "Atlas Güvenlik 2026 Sözleşmesi"
        
        public string? Description { get; set; }
        
        // Hangi şirketle/ustayla yapıldı? (Opsiyonel olarak ServiceProvider a bağlanabilir)
        public int? ServiceProviderId { get; set; }
        [ForeignKey("ServiceProviderId")]
        public virtual ServiceProvider? ServiceProvider { get; set; }

        public string? VendorName { get; set; } // Sistem dışı bir firmaysa manuel isim girilir
        public string? VendorContactPhone { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal MonthlyCost { get; set; } // Aylık ödeme tutarı

        public bool IsActive { get; set; } = true;

        public string? ContractDocumentUrl { get; set; } // Islak imzalı sözleşme PDF
    }
}
