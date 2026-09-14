using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.Finance
{
    // Müşteri / Ev Sahibi Cari Hesabı
    public class CustomerCurrentAccount : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string CustomerUserId { get; set; }
        public ApplicationUser CustomerUser { get; set; }

        // Bize olan toplam güncel borcu. (Ev sahibi katılım payı vs.)
        // Pozitif = Müşteri bize borçlu, Negatif = Müşteri alacaklı (Fazla ödeme/Avans)
        public decimal CurrentBalance { get; set; } = 0; 
        
        public ICollection<CustomerAccountTransaction> Transactions { get; set; }
    }

    public class CustomerAccountTransaction : BaseEntity
    {
        public int CustomerCurrentAccountId { get; set; }
        public CustomerCurrentAccount CustomerCurrentAccount { get; set; }

        public int? FinanceCategoryId { get; set; } // Hangi kalem? (Kentsel Dönüşüm Katılım Payı vb.)
        public FinanceCategory FinanceCategory { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        
        // İşlem tutarı
        public decimal Amount { get; set; }
        public bool IsDebtToUs { get; set; } // true ise Müşteri bize borçlandı (Fatura kestik), false ise müşteri ödedi (Tahsilat)

        public decimal BalanceAfterTransaction { get; set; }

        public string Description { get; set; }
        public string DocumentReference { get; set; }
        
        public string CreatedByUserId { get; set; }
    }
}
