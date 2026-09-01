using System;
using GMK360.Core.Entities.Identity;

namespace GMK360.Core.Entities
{
    public class InvoiceRecord : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string InvoiceNumber { get; set; } // Örn: EML-2024-001
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; } // KDV
        
        public string Description { get; set; } // Örn: "Aylık Cüzdan Aidat Kesintisi" Veya "Paket Satın Alımı"
        
        public bool IsGenerated { get; set; } = false; // E-Fatura portalında oluşturuldu mu?
        public string? PdfUrl { get; set; } // İndirilebilir PDF linki
        
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    }
}
