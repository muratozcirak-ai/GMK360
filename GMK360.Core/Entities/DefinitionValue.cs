namespace GMK360.Core.Entities
{
    public class DefinitionValue : BaseEntity
    {
        public int CategoryId { get; set; }
        public DefinitionCategory Category { get; set; }

        public string Name { get; set; } // Örn: Satılık, Kiralık, Daire, Kombi, Asansör
        public string SystemCode { get; set; } // Opsiyonel, özel işlemler için
        public int Order { get; set; } // Ekranda gösterim sırası
        
        // Dinamik Matris Konfigürasyonu
        public bool HasCount { get; set; } // Bu özellik için Adet girişi istenecek mi? (Örn: Banyo için true)
        public string SubOptions { get; set; } = string.Empty; // Virgülle ayrılmış alt özellikler. Örn: "Kabin, Ebeveyn" veya "Cam Balkon"
        
        // Kullanıcı Özel Metin Girişi
        public bool RequiresTextInput { get; set; } // Bu şık seçildiğinde kullanıcıdan metin girmesi istenecek mi? (Örn: "Özellik text box olmalı")
    }
}
