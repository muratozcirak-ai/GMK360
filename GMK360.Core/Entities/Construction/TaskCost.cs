using System;

namespace GMK360.Core.Entities.Construction
{
    public class TaskCost : BaseEntity
    {
        public int PhaseTaskId { get; set; }
        public PhaseTask PhaseTask { get; set; }

        public int CostCategoryId { get; set; }
        public CostCategory CostCategory { get; set; }

        public string Title { get; set; } // Örn: 2 Usta Yevmiyesi, Çit Kiralama
        
        public decimal Amount { get; set; } // Toplam Tutar (Maliyet)
        

        // --- KABA İNŞAAT VE METRAJ TAKİBİ ---
        public decimal? Quantity { get; set; } // Sipariş/Talep Edilen Miktar (Örn: 1500)
        public decimal? DeliveredQuantity { get; set; } // Şantiyeye Teslim Alınan Miktar (Örn: 350)
        public string Unit { get; set; } // Birim (Örn: m3, m2, Ton, Adet, mtül)


        
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        public string SupplierName { get; set; } // Kime ödendi
        public string Notes { get; set; }

        // --- SATIN ALMA & ONAY SİSTEMİ (Partner/Ortak Onayları) ---
        public string ApprovalStatus { get; set; } = "Onay Bekliyor"; // Onay Bekliyor, Onaylandı, Reddedildi, Onay Gerektirmez
        public string ApprovedByUserId { get; set; } // Onaylayan kişinin ID'si
        public DateTime? ApprovedAt { get; set; }
        public int? LinkedSystemMeetingId { get; set; } // Eğer bu alım için özel bir toplantı/karar açıldıysa

    }
}
