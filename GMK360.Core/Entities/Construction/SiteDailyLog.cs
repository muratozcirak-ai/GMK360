using System;
namespace GMK360.Core.Entities.Construction
{
    public class SiteDailyLog : BaseEntity
    {
        public int ProjectId { get; set; }
        public int AgencyId { get; set; }
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        public string? ReporterUserId { get; set; }
        
        public string? WeatherCondition { get; set; }
        public string? GeneralProgress { get; set; } // O gün ne yapıldı?
        
        // "Ustanın kahraman olmasını engelleyen, şefin kendini koruduğu yer"
        public string? ObstaclesAndDelays { get; set; } // Engeller, Gecikmeler ve Sebepleri
        public string? MaterialNeeds { get; set; } // Acil Malzeme İhtiyaçları
        
        public bool IsReadByAdmin { get; set; } = false;
    }
}
