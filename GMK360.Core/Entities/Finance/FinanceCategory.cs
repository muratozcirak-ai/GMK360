using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Finance
{
    public enum FinanceCategoryType
    {
        Income,  // Gelir / Tahsilat
        Expense  // Gider / Ödeme
    }

    // Merkezin belirlediği tüm Gelir/Gider kalemleri (Tek Düzen Hesap / Kategori)
    public class FinanceCategory : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public FinanceCategoryType Type { get; set; }
        public string Name { get; set; } // Örn: Kentsel Dönüşüm Katılım Payı, Malzeme Alımı, Taşeron Hakedişi
        public string Code { get; set; } // Opsiyonel Muhasebe Kodu Örn: 120.01
        
        // Bu kalemlerin nerelerde varsayılan olarak çıkacağını kontrol etmek için etiketler
        public bool IsDefaultForMaterialReceipt { get; set; } // Mal kabulde otomatik seçilsin mi?
        public bool IsDefaultForSubcontractor { get; set; } // Taşeron hakedişinde gelsin mi?
    }
}
