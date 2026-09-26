using System.Collections.Generic;
using System;

namespace GMK360.Core.Entities
{
    public class SystemLegalDocumentTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? LegalReference { get; set; } 
        public string? IssuedBy { get; set; }
        
        public string? Category { get; set; } 
        
        // Removed TargetModule, Stage, DisplayOrder, IsMandatory
        // Kept simple fields representing the raw global document template
    }
}
