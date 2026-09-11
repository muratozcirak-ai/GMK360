using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public class SpaceFixture : BaseEntity
    {
        public int UnitSpaceId { get; set; }
        public virtual UnitSpace UnitSpace { get; set; }

        public string Category { get; set; } // Vitrifiye, Dograma, Tesisat vb.
        public string ItemName { get; set; } // Lavabo Bataryasi, Asansor vb.
        
        public double Quantity { get; set; }
        public string Unit { get; set; } // Adet, Set, m2
        
                public string? StandardBrand { get; set; }
        public string? StandardModel { get; set; }
        
        // CRM Integration
        public int? SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual ServiceProvider? Supplier { get; set; }
        public string? SupplierName { get; set; } // Fallback for unregistered
        
        public int? TechnicalServiceId { get; set; }
        [ForeignKey("TechnicalServiceId")]
        public virtual ServiceProvider? TechnicalService { get; set; }
        public string? TechnicalServiceName { get; set; } // Fallback for unregistered

        public string? MaintenanceNotes { get; set; }
        
        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        
        public decimal? EstimatedPrice { get; set; }
        public decimal? TotalPrice { get; set; }

        public bool IsCustomizable { get; set; } = true;
        
        public virtual ICollection<MaterialOption> Options { get; set; } = new List<MaterialOption>();
    }
}
