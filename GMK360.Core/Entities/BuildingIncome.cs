using System;

namespace GMK360.Core.Entities
{
    public class BuildingIncome : BaseEntity
    {
        public int? HousingComplexId { get; set; } // Site geneli ise (Örn: Site dükkanı)
        public virtual HousingComplex? HousingComplex { get; set; }

        public int? BuildingId { get; set; } // Sadece binayı ilgilendiriyorsa (Örn: Çatı reklam geliri)
        public virtual Building? Building { get; set; }

        public string IncomeType { get; set; } = null!; // "Kira", "Hurda", "Reklam", "Gecikme Cezası", "Faiz"
        
        public string Title { get; set; } = null!; // "A Blok Altı Dükkan Kirası"
        
        public string? Description { get; set; } // Detaylı açıklama
        
        public decimal Amount { get; set; }
        
        public DateTime Date { get; set; } = DateTime.Now;

        // Geliri ödeyen tarafın (Örn: Dükkan kiracısı, Reklam şirketi) bilgisi
        public string? PayerName { get; set; }
        
        public string? ReceiptDocumentUrl { get; set; } // Makbuz veya kontrat PDF
    }
}
