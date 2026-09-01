using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }

    public class ServiceAppointment : BaseEntity
    {
        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }

        public int ServiceProviderId { get; set; }
        public ServiceProvider ServiceProvider { get; set; }

        public DateTime AppointmentDate { get; set; }
        
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public decimal? PriceOffer { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; } // Yanmış priz vb. fotoğrafı için
    }
}
