using System;
namespace GMK360.Core.Entities.Finance
{
    public class ProjectCashRequest : BaseEntity
    {
        public int ProjectId { get; set; }
        public int AgencyId { get; set; }
        public string? RequestedByUserId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Onaylandı, Reddedildi
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedDate { get; set; }
    }
}
