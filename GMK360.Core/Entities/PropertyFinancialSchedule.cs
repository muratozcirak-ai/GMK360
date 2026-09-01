using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public enum ScheduleStatus
    {
        Pending = 1,
        Paid = 2,
        Overdue = 3
    }

    public class PropertyFinancialSchedule : BaseEntity
    {
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }

        // Sorumlu kullanıcının (Kiracı veya Ev Sahibi) sistemdeki ID'si
        public string UserId { get; set; } 

        public int ObligationTypeId { get; set; }
        [ForeignKey("ObligationTypeId")]
        public virtual FinancialObligationType ObligationType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public ScheduleStatus Status { get; set; } = ScheduleStatus.Pending;

        public bool IsNotificationSent { get; set; } = false;
    }
}
