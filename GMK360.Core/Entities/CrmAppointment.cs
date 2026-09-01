using System;

namespace GMK360.Core.Entities
{
    public class CrmAppointment : BaseEntity
    {
        public int CrmContactId { get; set; }
        public CrmContact CrmContact { get; set; }

        public int? PropertyId { get; set; }
        public Property Property { get; set; }

        public string Title { get; set; } // Örn: Kadıköy Daire Gösterimi
        public string AppointmentType { get; set; } // DaireGosterme, TapuRandevusu, KaporaAlma
        
        public DateTime AppointmentDate { get; set; }
        
        public string Notes { get; set; }
    }
}
