using System;

namespace GMK360.Web.ViewModels
{
    public class AddPropertyViewModel
    {
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public int NeighborhoodId { get; set; }
        public int StreetId { get; set; }
        
        // Null ise yeni bina ekleniyor demektir
        public int? ExistingBuildingId { get; set; }
        
        public string? NewBuildingName { get; set; }
        public string DoorNumber { get; set; }
        
        public int RoleType { get; set; } // 1 = Owner, 2 = Tenant
        public int PropertyTypeId { get; set; }
        
        public string? RoomCount { get; set; }
        public int? NetArea { get; set; }
        public int? GrossArea { get; set; }
    }
}
