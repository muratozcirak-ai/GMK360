namespace GMK360.Core.Entities
{
    public class PropertyFeature : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int DefinitionValueId { get; set; }
        public DefinitionValue DefinitionValue { get; set; }

        public string Value { get; set; } // "Var", "Yok", "Açık", "Kapalı" (Geriye uyumluluk veya düz özellikler için)
        
        // Dinamik Matris Verileri
        public int? Count { get; set; } // Kaç Adet? (Örn: 2 Banyo)
        public string SelectedSubOptions { get; set; } // Seçilen alt özellikler (Örn: "Kabin, Ebeveyn")
        public string Note { get; set; } // Özel Not (Örn: "Akülüdür")
    }
}
