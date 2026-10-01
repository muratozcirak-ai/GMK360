using System;
namespace GMK360.Core.Entities.Logistics
{
    public class VehicleExpense : BaseEntity
    {
        public int VehicleId { get; set; }
        public CompanyVehicle Vehicle { get; set; }
        
        public int AgencyId { get; set; }
        public string ExpenseType { get; set; } // Yakıt, Bakım, OGS vb.
        public decimal Amount { get; set; }
        public string? ReceiptNumber { get; set; }
        public string? ReceiptPhotoUrl { get; set; }
        public string? ReportedBy { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    }
}
