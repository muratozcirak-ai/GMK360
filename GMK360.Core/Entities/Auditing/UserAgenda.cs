using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum AgendaType
    {
        General = 1,
        Appointment = 2,
        JobRequest = 3, // İhale / Talep
        Payment = 4
    }

    public class UserAgenda : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public DateTime Date { get; set; } // Hatırlatma veya Ajanda Tarihi

        public AgendaType AgendaType { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}
