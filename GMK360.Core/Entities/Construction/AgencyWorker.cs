using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public class AgencyWorker : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string? UserId { get; set; }
        public GMK360.Core.Entities.Identity.ApplicationUser? User { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public string IdentityNumber { get; set; } // TC No
        public string PhoneNumber { get; set; }

        public string Profession { get; set; } // Meslek
        public string WorkerType { get; set; } = "Firma Personeli"; // Taşeron, Firma Personeli, Yevmiyeci
        public string? SubcontractorName { get; set; } // Eğer taşeron ise firma adı

        // Taşeronun rehber kaydı (Gölge kullanıcı bağlantısı için)
        public int? SubcontractorContactId { get; set; }
        public AgencyPhonebook SubcontractorContact { get; set; }

        public decimal DefaultDailyWage { get; set; }
        public decimal NetDailyWage { get; set; } // İşçinin Cebine Giren
        public decimal DailySgkCost { get; set; } // Şirketin SGK Yükü
        public bool IsActive { get; set; } = true;

        // YENİ EKLENEN ÇAVUŞ / EKİP MANTIĞI
        public string? TeamName { get; set; } // Örn: "Ahmet Usta Kalıp Ekibi"
        public int? ForemanId { get; set; } // Eğer bu kişi bir çavuşa bağlıysa
        
        [ForeignKey("ForemanId")]
        public AgencyWorker? Foreman { get; set; }
        
        public ICollection<DailyTimesheet> Timesheets { get; set; }
    }
}
