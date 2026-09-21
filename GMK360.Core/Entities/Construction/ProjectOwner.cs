using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class ProjectOwner : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public int ContactId { get; set; }
        public CrmContact Contact { get; set; }

        public string? FlatNumber { get; set; }
        public string? BlockName { get; set; }
        public string? UnitType { get; set; }
        public double? LandShare { get; set; } // Arsa Payı Oranı
        public bool IsCommitteeMember { get; set; } // Heyet/Yönetim Kurulu üyesi mi?

        public virtual ICollection<ProjectOwnerDebt> Debts { get; set; }
    }
}
