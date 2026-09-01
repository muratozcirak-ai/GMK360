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

        public string Value { get; set; } // "Var", "Yok", "Açık", "Kapalı"
        
        // Dinamik Matris Verileri
        public int? Count { get; set; } // Kaç Adet? (Örn: 2 Havuz)
        public string SelectedSubOptions { get; set; } // Seçilen alt özellikler
        public string Note { get; set; } // Özel Not
    }
}
