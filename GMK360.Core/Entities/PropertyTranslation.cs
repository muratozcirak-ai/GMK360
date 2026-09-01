namespace GMK360.Core.Entities
{
    public class PropertyTranslation : BaseEntity
    {
        public int PropertyId { get; set; }
        public Property Property { get; set; }
        
        public string LanguageCode { get; set; } // Örn: EN, RU, DE
        public string TranslatedTitle { get; set; }
        public string TranslatedDescription { get; set; }
    }
}
