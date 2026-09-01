using System.Collections.Generic;

namespace GMK360.Core.Entities
{
    public class DefinitionCategory : BaseEntity
    {
        public string Name { get; set; } // Örn: İlan Durumu, İlan Tipi, Isınma Tipi, İç Özellikler
        public string SystemCode { get; set; } // Örn: PropertyStatus, PropertyType, Heating, InteriorFeatures (Koda bağlamak için)

        // Hiyerarşi (Grup > Soru mantığı için)
        public int? ParentCategoryId { get; set; }
        public DefinitionCategory ParentCategory { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.InverseProperty("ParentCategory")]
        public ICollection<DefinitionCategory> SubCategories { get; set; }

        // Sınav Testi Mantığı
        public bool IsMultiSelect { get; set; } // true = Checkbox (Çoklu Seçim), false = Radio (Tekli Seçim)
        public FeatureTargetType TargetType { get; set; } = FeatureTargetType.Both; // Hangi varlığa ait olduğu
        public bool IsMediaTag { get; set; } // Medya galerisinde fotoğraf etiketlemek için kullanılacak mı?

        [System.ComponentModel.DataAnnotations.Schema.InverseProperty("Category")]
        public ICollection<DefinitionValue> Values { get; set; }
    }
}
