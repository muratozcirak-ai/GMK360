using System;

namespace GMK360.Core.Entities.Construction
{
    public class TaskDocument : BaseEntity
    {
        public int PhaseTaskId { get; set; }
        public PhaseTask PhaseTask { get; set; }

        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; } // PDF, Image, vs
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; }
    }
}
