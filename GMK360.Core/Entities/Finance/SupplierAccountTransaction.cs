using System;
using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities.Finance
{
    public enum SupplierTransactionType
    {
        PurchaseInvoice,      // Resmi Alış Faturası (Borcumuzu Artırır - A Damarı)
        OpenAccountPurchase,  // Şantiye Serbest Veresiye Alımı (Borcumuzu Artırır - B Damarı)
        PaymentMade,          // Ödeme Yapıldı (Borcumuzu Düşürür - C Damarı)
        RefundReceived,       // İade Alındı
        Adjustment            // Düzeltme
    }

    public class SupplierAccountTransaction : BaseEntity
    {
        public int SupplierCurrentAccountId { get; set; }
        public SupplierCurrentAccount SupplierCurrentAccount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public SupplierTransactionType Type { get; set; }
        
        public int? FinanceCategoryId { get; set; }
        public FinanceCategory FinanceCategory { get; set; }

        // Şantiye bağlantısı (Hangi şantiyeye alındı/verildi?)
        public int? ProjectId { get; set; }
        public ConstructionProject Project { get; set; }

        // İşlem tutarı
        public decimal Amount { get; set; }
        
        // İşlem sonrası bakiye snapshot'ı
        public decimal BalanceAfterTransaction { get; set; }

        // SERBEST YAZI: "3 top branda (Hasan usta teslim aldı)"
        public string Description { get; set; }
        public string DocumentReference { get; set; } // Fatura No, Sipariş No vb.
        
        public string CreatedByUserId { get; set; }
    }
}
