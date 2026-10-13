using System;
using GMK360.Core.Entities;

namespace GMK360.Core.Entities.Construction.Workflows
{
    public class MasterIsgQuestion : BaseEntity
    {
        public string PhaseCategory { get; set; } // Örn: "Temel", "TipKat", "Cati"
        public string QuestionText { get; set; }  // Sorunun kendisi
        public bool IsCritical { get; set; }      // TRUE ise şantiyeyi kilitler
    }
}
