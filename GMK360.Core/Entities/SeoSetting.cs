using System;

namespace GMK360.Core.Entities
{
    public class SeoSetting : BaseEntity
    {
        public string PagePath { get; set; } // Ornegin: "/", "/Contact", "/DigitalHome"
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Keywords { get; set; }
        public string? OgImage { get; set; }
    }
}
