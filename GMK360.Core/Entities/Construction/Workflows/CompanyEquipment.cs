using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class CompanyEquipment : BaseEntity
    {
        public string EquipmentCode { get; set; } // Örn: "VNC-01", "KPC-02"
        public string EquipmentName { get; set; } // Örn: "75 Tonluk Mobil Vinç", "JCB Kepçe"
        
        public string OwnershipType { get; set; } // "Özmal" (Kendi Garajımız), "Kiralık" (Taşeron/Dış Firma)
        public string PricingUnit { get; set; }   // Örn: "Saatlik", "Günlük", "Aylık"
        
        // Özmal ise amortisman+yakıt maliyeti, Kiralık ise firmanın kestiği saatlik fatura bedeli
        public decimal UnitCost { get; set; }     // Örn: 1.500 TL (Saatlik)
    }
}
