namespace GMK360.Core.Entities
{
    public class CrmDemand : BaseEntity
    {
        public int CrmContactId { get; set; }
        public CrmContact CrmContact { get; set; }

        public string Description { get; set; } // Örn: Kadıköy'de 3+1 daire arıyor
        public decimal MinBudget { get; set; }
        public decimal MaxBudget { get; set; }
        public string PreferredRegion { get; set; }
        
        public string Status { get; set; } // Bekliyor, Olumlu, Olumsuz, Kapatildi
    }
}
