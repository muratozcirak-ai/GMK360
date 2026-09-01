using System.Collections.Generic;
using GMK360.Core.Entities;

namespace GMK360.Web.Models.Dashboard
{
    public class ResidentDashboardViewModel
    {
        // Kullanıcının kendisine ait daire(ler)
        public List<BuildingUnit> MyUnits { get; set; } = new List<BuildingUnit>();

        // Apartman/Site ile ilgili genel bilgiler (Yöneticinin girdiği veriler)
        public List<Meeting> UpcomingMeetings { get; set; } = new List<Meeting>();
        public List<UnitDebt> UnpaidDebts { get; set; } = new List<UnitDebt>();
        public List<BuildingAnnouncement> Announcements { get; set; } = new List<BuildingAnnouncement>();

        // Çapraz Satış (Cross-Sell) CTA Mantığı
        // Eğer kullanıcı Gölge (Shadow) olarak girdiyse ve sadece aidat ödüyorsa, onu platformun diğer hizmetlerine (kredi, usta çağırma vb.) teşvik etmek için bayraklar
        public bool ShowRenovationCta { get; set; }
        public bool ShowPropertyEvaluationCta { get; set; }
        public bool ShowInsuranceCta { get; set; }
    }
}
