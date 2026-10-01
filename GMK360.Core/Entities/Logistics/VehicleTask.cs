using System;
namespace GMK360.Core.Entities.Logistics
{
    public class VehicleTask : BaseEntity
    {
        public int VehicleId { get; set; }
        public CompanyVehicle Vehicle { get; set; }
        
        public int AgencyId { get; set; }
        public string? DriverName { get; set; }
        public int? DestinationProjectId { get; set; }
        public string TaskDescription { get; set; }
        public DateTime TaskDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Bekliyor"; // Bekliyor, Yolda, Tamamlandı
    }
}
