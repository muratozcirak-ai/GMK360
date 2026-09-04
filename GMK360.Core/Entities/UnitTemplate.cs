using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Construction;

namespace GMK360.Core.Entities
{
    public class UnitTemplate : BaseEntity
    {
        public int ConstructionProjectId { get; set; }
        public ConstructionProject ConstructionProject { get; set; }

        public string Name { get; set; } // Örn: "A Blok Lüks 3+1"
        public string RoomLayout { get; set; } // Örn: "3+1"
        public string? Description { get; set; }

        public virtual ICollection<UnitTemplateSpace> Spaces { get; set; }
    }
}
