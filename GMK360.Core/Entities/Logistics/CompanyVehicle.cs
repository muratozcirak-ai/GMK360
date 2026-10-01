using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Logistics
{
    public class CompanyVehicle : BaseEntity
    {
        public int AgencyId { get; set; }
        public string PlateNumber { get; set; }
        public string? VehicleType { get; set; } // Pikap, Binek, Minibüs vs
        public string? BrandModel { get; set; }
        public string Status { get; set; } = "Aktif"; // Aktif, Bakımda, Pasif
        
        public ICollection<VehicleAssignment> Assignments { get; set; }
        public ICollection<VehicleTask> Tasks { get; set; }
        public ICollection<VehicleExpense> Expenses { get; set; }
    }

    public class VehicleAssignment : BaseEntity
    {
        public int VehicleId { get; set; }
        public CompanyVehicle Vehicle { get; set; }
        
        public int AgencyId { get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedToUserId { get; set; }
        
        public int? ProjectId { get; set; } // Hangi şantiyede?
        
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnDate { get; set; }
        public string? Notes { get; set; }
    }
}

