using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities.Construction
{
    public static class AgendaConstants
    {
        // Görüşme Tipleri
        public const byte TypeMusteriGorusmesi = 1;
        public const byte TypeTaseronToplantisi = 2;
        public const byte TypeSantiyeDenetimi = 3;
        public const byte TypeGenelNot = 4;

        // Durumlar
        public const byte StatusPlanlandi = 1;
        public const byte StatusTamamlandi = 2;
        public const byte StatusIptal = 3;
    }

    public class AgendaRecord : BaseEntity
    {
        // Çok kiracılı (Multi-tenant) yapı için zorunlu alan
        public int AgencyId { get; set; }
        [ForeignKey("AgencyId")]
        public virtual Agency Agency { get; set; }

        public string Title { get; set; } // Örn: "Hasan Bey'le Temel Betonu Ön Görüşmesi"
        public string Notes { get; set; } // Uzun hazırlık notları veya toplantı kararları
        
        // Zaman Planlaması (Geçmiş kayıt veya gelecek planı)
        public DateTime EventDate { get; set; } 
        
        // Görsel/Sunum Eklentisi (Örn: "/uploads/agenda/sozlesme-taslagi.jpg")
        public string ImageUrl { get; set; } 

        public byte RecordTypeId { get; set; } = AgendaConstants.TypeGenelNot;
        public byte StatusId { get; set; } = AgendaConstants.StatusPlanlandi;

        // Şantiye bağlantısı (Opsiyonel)
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual ConstructionProject Project { get; set; }
        
        // Firma / Usta / Kişi bağlantısı (Opsiyonel - Firma Rehberinden)
        public int? PhonebookId { get; set; }
        [ForeignKey("PhonebookId")]
        public virtual AgencyPhonebook Phonebook { get; set; }
    }
}
