using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class WorkItemSubcontractorContract : BaseEntity
    {
        public int ProjectId { get; set; }
        public int SubcontractorId { get; set; } // Taşeron Firma/Usta ID
        public string WorkItemCode { get; set; } // Örn: "DUVAR_YTONG_20"
        public decimal UnitPrice { get; set; }   // Örn: 150.00 TL (1 m2 işçilik fiyatı)
    }
}
