using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class WalletWithdrawalRequest : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public bool IsProcessed { get; set; } = false;
        public string? AdminNotes { get; set; }

        public bool RequiresInvoice { get; set; } = false; // Kurumsal üyeden Referans kazancı vb. çekimi için fatura zorunlu mu?

        public int? UploadedInvoiceDocumentId { get; set; } // Kullanıcının yüklediği SystemDocument faturası
    }
}
