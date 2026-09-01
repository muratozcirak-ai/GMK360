namespace GMK360.Core.Entities
{
    public class AgencyWebBlock : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        // Blok türü: "HeroSlider", "FeatureBanner", "ProjectList", "HtmlContent"
        public string BlockType { get; set; } 
        
        public int OrderIndex { get; set; } // Sitede görünme sırası
        
        public string Title { get; set; }
        public string Subtitle { get; set; }
        
        public string ImageUrl { get; set; }
        public string TargetUrl { get; set; }
        
        public string ContentHtml { get; set; } // Özelleştirilmiş HTML veya yazı
        
        public bool IsActive { get; set; } = true;
    }
}
