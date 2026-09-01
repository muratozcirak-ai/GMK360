using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class ApiCredential : BaseEntity
    {
        // Hangi kurumsal ofis (şirket hesabı)?
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } // Sadece "Kurumsal/Admin" rolündeki kullanıcılar olmalı.

        // REST API İstekleri için 
        public string ApiKey { get; set; } // Sistem tarafından üretilen rastgele key (Guid)
        public string SecretKey { get; set; } // Sadece ilk oluşturulduğunda bir kez gösterilir

        // XML Aktarımı için spesifik ve benzersiz bir link ucu (Örn: /export/xml/a8f9b2...)
        public string XmlExportToken { get; set; } 

        // API Kullanım sınırlandırması ve güvenliği için
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsedAt { get; set; } // En son ne zaman API isteği yaptı?
    }
}
