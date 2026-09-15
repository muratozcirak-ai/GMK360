using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public class AgencyWorker : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";

        public string IdentityNumber { get; set; } // TC No
        public string PhoneNumber { get; set; }

        public string Profession { get; set; } // Meslek
        public string WorkerType { get; set; } = "Firma Personeli"; // Taşeron, Firma Personeli, Yevmiyeci
        public string SubcontractorName { get; set; } // Eğer taşeron ise firma adı

        // Taşeronun rehber kaydı (Gölge kullanıcı bağlantısı için)
        public int? SubcontractorContactId { get; set; }
        public AgencyPhonebook SubcontractorContact { get; set; }

        public decimal DefaultDailyWage { get; set; }
        public decimal NetDailyWage { get; set; } // İşçinin Cebine Giren
        public decimal DailySgkCost { get; set; } // Şirketin SGK Yükü // Standart yevmiyesi
        public bool IsActive { get; set; } = true;

        public ICollection<DailyTimesheet> Timesheets { get; set; }
    }
}
