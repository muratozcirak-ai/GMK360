using System;

namespace GMK360.Core.Entities.Construction
{
    public class TaskProgressLog : BaseEntity
    {
        public int ConstructionTaskId { get; set; }
        public ConstructionTask ConstructionTask { get; set; }

        public string Note { get; set; } // Örn: "Mutfak fayansları tamamlandı, derz dolgusu yarına kaldı."
        public string PhotoUrl { get; set; } // Ustadan gelen fotoğraf

        public DateTime LogDate { get; set; }
    }
}
