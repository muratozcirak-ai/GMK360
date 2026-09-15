using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities.Finance
{
    public class SubcontractorContract : BaseEntity
    {
        public int AgencyId { get; set; } // Şantiye / Müteahhit
        public Agency Agency { get; set; }

        public int ProjectId { get; set; }
        public GMK360.Core.Entities.Construction.ConstructionProject Project { get; set; }

        public int PhonebookContactId { get; set; } // Rehberdeki Taşeron
        public AgencyPhonebook PhonebookContact { get; set; }

        public string Title { get; set; } // Sözleşme Adı (örn: A Blok Demir İşçiliği)
        public string? Description { get; set; } // İşin kapsamı
        
        public DateTime ContractDate { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public decimal TotalAmount { get; set; } // Toplam Bedel
        public string Currency { get; set; } = "TRY";

        // true = Biz onlara ödeyeceğiz (Gider Sözleşmesi - Normal Taşeron)
        // false = Onlar bize ödeyecek (Gelir Sözleşmesi - Hurda/Yıkım Firması)
        public bool IsExpenseContract { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public ICollection<ContractPhase> Phases { get; set; }
        public virtual ICollection<SubcontractorHakedis> Hakedisler { get; set; }
    }
}

