using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities.Finance
{
    public class SubcontractorContract : BaseEntity
    {
        public int AgencyId { get; set; } // Þantiye / Müteahhit
        public Agency Agency { get; set; }

        public int ProjectId { get; set; }
        public GMK360.Core.Entities.Construction.ConstructionProject Project { get; set; }

        public int PhonebookContactId { get; set; } // Rehberdeki Taþeron
        public AgencyPhonebook PhonebookContact { get; set; }

        public string Title { get; set; } // Sözleþme Adý (örn: A Blok Demir Ýþçiliði)
        public string? Description { get; set; } // Ýþin kapsamý
        
        public DateTime ContractDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal TotalAmount { get; set; } // Toplam Bedel
        public string Currency { get; set; } = "TRY";

        // true = Biz onlara ödeyeceðiz (Gider Sözleþmesi - Normal Taþeron)
        // false = Onlar bize ödeyecek (Gelir Sözleþmesi - Hurda/Yýkým Firmasý)
        public bool IsExpenseContract { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public ICollection<ContractPhase> Phases { get; set; }
        public ICollection<ProgressPayment> ProgressPayments { get; set; }
    }
}

