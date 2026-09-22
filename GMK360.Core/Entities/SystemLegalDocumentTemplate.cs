using System;

namespace GMK360.Core.Entities
{
    public class SystemLegalDocumentTemplate : BaseEntity
    {
        public string Name { get; set; }
        public string? Stage { get; set; }
        
        // e.g. "Construction", "RealEstate", "ServiceProvider"
        public string TargetModule { get; set; } 
        
        public bool IsMandatory { get; set; } = true;
        public string? LegalReference { get; set; } // Hangi kanun, tzK
        
        public string? IssuedBy { get; set; } // Kimden Alnr? (Belediye, Ticaret Bakanl, zel Firma vb.)
    }
}
