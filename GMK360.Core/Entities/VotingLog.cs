using System;

namespace GMK360.Core.Entities
{
    public class VotingLog : BaseEntity
    {
        public int BuildingManagementId { get; set; }
        
        public string Title { get; set; } // Örn: "Çatı Tadilatı Oylaması"
        public string Description { get; set; }
        
        public string ResidentPhoneNumber { get; set; }
        public string ResidentUnitNumber { get; set; } // Daire No
        
        public string SentOtpCode { get; set; }
        public bool IsApproved { get; set; }
        
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
        
        // Değiştirilemez delil (Audit Trail) - Bu hash, IP + Timestamp + OTP + Sonuç kullanılarak oluşturulup değiştirilemezliği kanıtlar
        public string AuditHash { get; set; }
    }
}
