using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class DxfUnitTypeProfile : BaseEntity
    {
        public int ProjectId { get; set; }
        
        // Örn: "A Tipi (3+1 Kuzey Cephe)", "Dükkan Tipi 1", "Ortak Alan: Kapalı Otopark"
        public string ModelName { get; set; }
        public string Category { get; set; } // Daire, Ticari, Ortak Alan
        
        public decimal TotalGrossArea { get; set; }
        public decimal TotalNetArea { get; set; }
        public int TotalSpaceCount { get; set; }
        
        // Navigation
        public ICollection<DxfUnitSpace> Spaces { get; set; } = new List<DxfUnitSpace>();
    }
}
