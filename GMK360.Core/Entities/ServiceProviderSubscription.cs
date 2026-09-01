using System;

namespace GMK360.Core.Entities
{
    public enum ProviderSubscriptionTier
    {
        BasicLocal, // Sadece Kendi Mahallesi ve komşu mahalleler
        ProDistrict, // Tüm İlçe
        EnterpriseCity // Tüm İstanbul/Şehir
    }

    public class ServiceProviderSubscription : BaseEntity
    {
        public string ProviderUserId { get; set; }
        
        public ProviderSubscriptionTier Tier { get; set; } = ProviderSubscriptionTier.BasicLocal;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
        
        public decimal PaidAmount { get; set; }
        public string PaymentReference { get; set; } // Dekont / İşlem No
    }
}
