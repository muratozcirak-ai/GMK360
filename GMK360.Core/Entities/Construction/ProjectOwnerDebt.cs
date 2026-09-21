using System;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectOwnerDebt : BaseEntity
    {
        public int ProjectOwnerId { get; set; }
        public ProjectOwner ProjectOwner { get; set; }

        public decimal DebtAmount { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
    }
}
