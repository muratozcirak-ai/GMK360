using System;

namespace GMK360.Core.Entities.Finance
{
    public enum SupplierTransactionType
    {
        PurchaseInvoice,    // Alış Faturası (Borcumuzu Artırır)
        PaymentMade,        // Ödeme Yapıldı (Borcumuzu Düşürür)
        RefundReceived,     // İade Alındı (Borcumuzu Artırır / Avans iadesi)
        Adjustment          // Düzeltme
    }

    public class SupplierAccountTransaction : BaseEntity
    {
        public int SupplierCurrentAccountId { get; set; }
        public SupplierCurrentAccount SupplierCurrentAccount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public SupplierTransactionType Type { get; set; }
        
                public int? FinanceCategoryId { get; set; }
        public FinanceCategory FinanceCategory { get; set; }

        // İşlem tutarı
        public decimal Amount { get; set; }
        
        // İşlem sonrası bakiye snapshot'ı (opsiyonel ama sağlıklı takip için)
        public decimal BalanceAfterTransaction { get; set; }

        public string Description { get; set; }
        public string DocumentReference { get; set; } // Fatura No, Sipariş No vb.
        
        public string CreatedByUserId { get; set; }
    }
}

