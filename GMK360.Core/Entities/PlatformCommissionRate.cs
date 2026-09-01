namespace GMK360.Core.Entities
{
    public class PlatformCommissionRate : BaseEntity
    {
        public PaymentContextType ContextType { get; set; } // Hangi işleme yüzde kaç?
        
        public decimal Percentage { get; set; } // Örn: 2.5 (Yani %2.5)
        public decimal FixedFee { get; set; } // Sabit ücret varsa

        // İsteğe bağlı: Belirli bir ustaya/kuruma özel komisyon tanımlanabilir
        public string? SpecificUserId { get; set; }
        public Identity.ApplicationUser SpecificUser { get; set; }
    }
}
