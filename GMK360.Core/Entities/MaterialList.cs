using System;
using System.Collections.Generic;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class MaterialList : BaseEntity
    {
        // Usta (Kullanıcı)
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public int? CrmAppointmentId { get; set; } // İsteğe bağlı olarak Müşteri İşine/Ajandaya bağlamak için
        public CrmAppointment CrmAppointment { get; set; }

        public string Title { get; set; } // Örn: "Kadıköy Boya İşi Malzemeleri"
        
        // Durum (Bekliyor, Teklifler Geldi, Tamamlandı vs.)
        public string Status { get; set; } = "Bekliyor";

        public ICollection<MaterialListItem> Items { get; set; }
        public ICollection<MaterialListOffer> Offers { get; set; }
    }
}
