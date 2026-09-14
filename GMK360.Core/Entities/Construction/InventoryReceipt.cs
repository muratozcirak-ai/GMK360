using System;
using System.Collections.Generic;

namespace GMK360.Core.Entities.Construction
{
    // Mal Kabul (İrsaliye / Fatura ile Giriş)
    public class InventoryReceipt : BaseEntity
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public int WarehouseId { get; set; } // Hangi Şantiye / Depoya İndi?
        public Warehouse Warehouse { get; set; }

        public int? B2BQuoteRequestId { get; set; } // Hangi satın alma kararına istinaden geldi?
        public GMK360.Core.Entities.B2B.B2BQuoteRequest B2BQuoteRequest { get; set; }

        public string SupplierName { get; set; } // Tedarikçi Firma / Şahıs
        public string DocumentNumber { get; set; } // İrsaliye / Fatura No
        
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow; // Teslimat Tarihi
        public string Notes { get; set; } // Teslim alan notları (Örn: 2 tanesi kırık çıktı iade edildi)

                public string Status { get; set; } = "Draft"; // Draft, Approved (Stoka işlendi)

        public string? AssignedUserId { get; set; } // Teslim Alması Beklenen Kişi
        public GMK360.Core.Entities.Identity.ApplicationUser? AssignedUser { get; set; }

        public ICollection<InventoryReceiptItem> Items { get; set; }
    }

    public class InventoryReceiptItem : BaseEntity
    {
        public int InventoryReceiptId { get; set; }
        public InventoryReceipt InventoryReceipt { get; set; }

        public int MaterialCatalogId { get; set; }
        public MaterialCatalog MaterialCatalog { get; set; }

        public decimal Quantity { get; set; }
        public decimal? UnitPrice { get; set; } // Şantiye için zorunlu değil
    }
}


