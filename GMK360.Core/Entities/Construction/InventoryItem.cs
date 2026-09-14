using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    public enum InventoryItemType
    {
        SarfMalzeme = 1, // Tüketilen (Çimento, Kum)
        Demirbas = 2     // Geri dönen/Gezici (Kalıp, Jeneratör, Hilti)
    }

    public enum InventoryEntryMethod
    {
        Devir = 1,      // Sistem öncesi mevcut mal varlığı
        SatinAlma = 2   // Sistem üzerinden alınmış
    }

    public class InventoryItem : BaseEntity
    {
        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }

        public int? MaterialCatalogId { get; set; }
        public MaterialCatalog MaterialCatalog { get; set; }

        public string Name { get; set; }
        public InventoryItemType ItemType { get; set; }
        public InventoryEntryMethod EntryMethod { get; set; }

        // --- Esnek (Nullable) Alanlar ---
        public string? Brand { get; set; }           // Marka (İsteğe bağlı)
        public string? Model { get; set; }           // Model (İsteğe bağlı)
        public string? SerialNumber { get; set; }    // Seri/Barkod No
        public string? SupplierName { get; set; }    // Alınan Yer (Devirde boş kalabilir)
        public string? ServiceContact { get; set; }  // Teknik Servis İletişim (Arıza olunca girilebilir)

        public decimal Quantity { get; set; }
        public string Unit { get; set; }            // Adet, m2, Ton, Lt
        
        // Demirbaş eğer şantiyeye verildiyse güncel durumu
        public string CurrentStatus { get; set; } = "Depoda"; // Depoda, Şantiyede, Arızalı/Serviste, Zayi
        
        public ICollection<InventoryTransaction> Transactions { get; set; }
    }
}

