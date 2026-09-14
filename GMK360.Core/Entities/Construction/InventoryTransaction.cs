using System;

namespace GMK360.Core.Entities.Construction
{
    public enum TransactionType
    {
        Giris = 1,       // Depoya girdi (Alım veya Devir)
        Cikis = 2,       // Tüketildi (Sarf)
        Sevk = 3,        // Şantiyeye gönderildi (Demirbaş)
        Iade = 4,        // Şantiyeden depoya döndü (Demirbaş)
        FireZayi = 5,    // Kırıldı/Kayboldu
        Tamir = 6,       // Tamire / Servise gitti
        Imha = 7         // Hurda / İmha (Vergiden düşülecek)
    }

    public class InventoryTransaction : BaseEntity
    {
        public int InventoryItemId { get; set; }
        public InventoryItem InventoryItem { get; set; }

        public TransactionType Type { get; set; }
        public decimal Quantity { get; set; }
        public DateTime TransactionDate { get; set; }

        public int? PhaseTaskId { get; set; }
        public PhaseTask PhaseTask { get; set; }

        public string Description { get; set; }
        public string HandledByUserId { get; set; }
    }
}
