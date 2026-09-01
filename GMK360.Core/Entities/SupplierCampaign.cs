using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class SupplierCampaign : BaseEntity
    {
        public string SupplierUserId { get; set; }
        public ApplicationUser SupplierUser { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }
}
