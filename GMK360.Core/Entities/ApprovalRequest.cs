using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    /// <summary>
    /// Kritik işlemlerde (fatura ters kaydı, borçlu devir, yönetici devri vb.) 2 Aza (BoardMember) onayı kuyruğu.
    /// </summary>
    public class ApprovalRequest : BaseEntity
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; } = null!;

        [Required, MaxLength(450)]
        public string RequestedByUserId { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string RequestType { get; set; } = string.Empty; // "REVERSE_EXPENSE", "DEBT_TRANSFER", "MANAGER_TRANSFER"

        [Required, MaxLength(80)]
        public string TargetEntityName { get; set; } = string.Empty;

        public int TargetEntityId { get; set; }

        public string PayloadJson { get; set; } = "{}";

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Status { get; set; } = "PENDING"; // "PENDING", "EXECUTED", "REJECTED"

        public int RequiredApprovals { get; set; } = 2;
        public int ApprovalCount { get; set; } = 0;

        public string ApprovedByJson { get; set; } = "[]";
        public DateTime? ExecutedAt { get; set; }
    }
}
