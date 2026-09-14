using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities.Finance
{
    public class SupplierCurrentAccount : BaseEntity
    {
        public int AgencyId { get; set; } // Þantiyenin (Bizim) ID'miz
        public Agency Agency { get; set; }

        public int PhonebookContactId { get; set; } // Rehberdeki kiþi/firma
        public AgencyPhonebook PhonebookContact { get; set; }

        // Bizim tedarikçiye olan toplam güncel borcumuz (Alacak bakiye). 
        // Pozitif = Biz borçluyuz, Negatif = Biz alacaklýyýz (Avans verdiysek)
        public decimal CurrentBalance { get; set; } = 0; 
        
        public ICollection<SupplierAccountTransaction> Transactions { get; set; }
        public ICollection<SupplierPayment> Payments { get; set; }
    }
}

