using System.ComponentModel.DataAnnotations.Schema;

using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class ComplexFeature : BaseEntity
    {
        public int ComplexId { get; set; }
        [ForeignKey("ComplexId")]
        public virtual Building Complex { get; set; }
        
        [NotMapped]
        public int BuildingId { get => ComplexId; set => ComplexId = value; }

        public int DefinitionValueId { get; set; }
        public DefinitionValue DefinitionValue { get; set; }

        public string Value { get; set; } // "Var", "Yok", "AÃ§Ä±k", "KapalÄ±"
        
        // Dinamik Matris Verileri
        public int? Count { get; set; } // KaÃ§ Adet? (Ã–rn: 2 Havuz)
        public string SelectedSubOptions { get; set; } // SeÃ§ilen alt Ã¶zellikler
        public string Note { get; set; } // Ã–zel Not
    }
}

