using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities.B2B
{
    // Her modülde kullanılabilen Merkezi İhale / Fiyat İsteme Tablosu
    public class B2BQuoteRequest : BaseEntity
    {
        public int RequesterAgencyId { get; set; }
        public Agency RequesterAgency { get; set; }
        public string RequesterUserId { get; set; }

        // Nereden istendi? (ConstructionTask, Renovation, MaterialList vs.)
        public string SourceModule { get; set; } 
        public int SourceReferenceId { get; set; } 

        public string Title { get; set; } // Örn: 2 Ton 14'lük Demir
        public string Description { get; set; }
        
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Draft";

        // Teklif Davetleri
        public virtual ICollection<B2BQuoteInvite> Invites { get; set; } = new List<B2BQuoteInvite>();
        public virtual ICollection<B2BQuoteItem> Items { get; set; } = new List<B2BQuoteItem>();
    }
}

