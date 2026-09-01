using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public enum ManagementStaffRole
    {
        Manager,       // Yönetici (Tam yetki, finans görebilir)
        BoardMember,   // Aza (Yönetim Kurulu Üyesi)
        Auditor,       // Denetmen (Sadece okuma / rapor)
        Worker         // Çalışan (Bahçıvan, Temizlikçi, Güvenlik) - Sisteme girmesine gerek yok, maaş takibi için
    }

    public class ManagementMember : BaseEntity
    {
        public int? HousingComplexId { get; set; }
        public HousingComplex? HousingComplex { get; set; }

        public int? BuildingId { get; set; }
        public Building? Building { get; set; }

        public string? UserId { get; set; } // Çalışan sisteme girmiyorsa null kalabilir
        public ApplicationUser? User { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ManagementStaffRole Role { get; set; }
        public string Title { get; set; } // "Bahçıvan", "Gece Bekçisi", "A Blok Temsilcisi"

        // Finans ve Bordro Bilgileri (Worker için önemli)
        public decimal MonthlySalary { get; set; }
        public string? SgkNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
    }
}
