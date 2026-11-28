using System.ComponentModel.DataAnnotations.Schema;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class BuildingManager : BaseEntity
    {
        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public virtual Building Building { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public BuildingManagerRole Role { get; set; }

        // Yönetim devredildiğinde false yapılır, geçmiş veriyi tutmak için silinmez.
        public bool IsActive { get; set; } = true;

        // Yetki & İmza Kuralı (Tek Başına / Müşterek / Yetkisiz)
        public SignatureAuthorityType SignatureAuthorityType { get; set; } = SignatureAuthorityType.None;

        // KMK Md. 34: Yönetici/Kurul dışarıdan seçilebilir; Md. 41: Denetçi mutlaka kat maliki olmalıdır
        public bool IsExternalAppointee { get; set; } = false;

        // Görev Dönemi (KMK gereği olağan görev süresi 1 yıl)
        public DateTime? TermStartDate { get; set; }
        public DateTime? TermEndDate { get; set; }

        // Atandığı Dijital Karar Defteri Kaydı (SystemMeeting.Id)
        public int? AppointmentMeetingId { get; set; }

        // Daire Sakinlerine Görünürlük (Yalnızca aktif yönetim iletişimini şeffaf paylaşmak için)
        public bool IsVisibleToResidents { get; set; } = true;

        [System.ComponentModel.DataAnnotations.MaxLength(200)]
        public string? CustomRoleNote { get; set; }

        // Kurul Hiyerarşi / Yedek Sırası (Örn: Aza 1, Aza 2 veya Yedek 1, Yedek 2)
        public int HierarchyOrder { get; set; } = 0;

        public bool IsTermExpired() => TermEndDate.HasValue && TermEndDate.Value.Date < DateTime.Today;

        public bool IsEffectiveActive() => IsActive && !IsTermExpired();

        public bool CanVoteBoardDecisions() => IsEffectiveActive() && GetHierarchyLevel(Role) <= 3;

        public static int GetHierarchyLevel(BuildingManagerRole role) => role switch
        {
            BuildingManagerRole.HeadManager => 1,
            BuildingManagerRole.ViceChair => 2,
            BuildingManagerRole.BoardMember => 3,
            BuildingManagerRole.ReserveMember => 4,
            BuildingManagerRole.Auditor => 5,
            _ => 3
        };

        public static string GetHierarchyLevelTitle(int level) => level switch
        {
            1 => "Seviye 1 — Başkan / Tek Yönetici",
            2 => "Seviye 2 — Başkan Yrd. / Muhasip",
            3 => "Seviye 3 — Asil Üye (Aza)",
            4 => "Seviye 4 — Yedek Üye",
            5 => "Seviye 5 — Denetçi (Malik Şart)",
            _ => "Kurul Üyesi"
        };

        public static string GetRoleLabel(BuildingManagerRole role) => role switch
        {
            BuildingManagerRole.HeadManager => "Yönetim Kurulu Başkanı",
            BuildingManagerRole.ViceChair => "Başkan Yardımcısı",
            BuildingManagerRole.BoardMember => "Yönetim Kurulu Üyesi (Aza)",
            BuildingManagerRole.ReserveMember => "Yedek Üye",
            BuildingManagerRole.Auditor => "Denetçi / Denetim Kurulu Üyesi",
            _ => "Kurul Üyesi"
        };

        public static string GetSignatureLabel(SignatureAuthorityType auth) => auth switch
        {
            SignatureAuthorityType.Single => "Tek Başına İmzaya Yetkili",
            SignatureAuthorityType.Joint => "Müşterek (Çift İmzalı) Yetkili",
            _ => "İmza Yetkisi Yok (Üye/Aza)"
        };
    }
}
