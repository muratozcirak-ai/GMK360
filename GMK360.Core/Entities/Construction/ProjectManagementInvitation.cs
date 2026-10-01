using System;
namespace GMK360.Core.Entities.Construction
{
    public class ProjectManagementInvitation : BaseEntity
    {
        public int ProjectId { get; set; }
        public int AgencyId { get; set; }
        public string ManagerName { get; set; }
        public string ManagerPhone { get; set; }
        public string? ManagerEmail { get; set; }
        public string InvitationToken { get; set; }
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Kabul Edildi
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
    }
}
