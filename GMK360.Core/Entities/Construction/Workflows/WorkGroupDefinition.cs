using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Workflows
{
    // Tanýmlama Tablosu: Taþeron Branþlarý / Ýþ Gruplarý (Hafriyatçý, Çatýcý, Satýn Alma vb.)
    public class WorkGroupDefinition : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Örn: Satýn Alma, Kalýpçý, Su Tesisatçýsý

        [MaxLength(250)]
        public string Description { get; set; } // Örn: Kaba inþaat kalýp iþlerini yapan ekip

        // Altýn Kural: Bu iþ grubunun ihaleye mi çýkacaðý yoksa þirket içi tedarik mi olduðu
        public bool IsProcurementOnly { get; set; } // True ise "Satýn Alma" demektir, taþerona gitmez.
        
        public bool IsActive { get; set; } = true;
    }
}
