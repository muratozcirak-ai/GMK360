using System;

namespace GMK360.Core.Entities
{
    public enum DocumentType
    {
        Invoice,          // Fatura
        Receipt,          // Makbuz
        ExpenseNote,      // Gider Pusulası
        Contract,         // Sözleşme
        Other             // Diğer
    }

    public class DocumentRegistry : BaseEntity
    {
        public OwnerType OwnerType { get; set; }
        public int? OwnerId { get; set; }

        public DocumentType Type { get; set; }
        public string DocumentNumber { get; set; } // Fatura/Makbuz No
        public DateTime DocumentDate { get; set; }
        
        public decimal? Amount { get; set; }
        public string Title { get; set; } // Örn: Asansör Motor Bakımı
        public string? Description { get; set; }
        
        public int? SystemDocumentId { get; set; } // Fiziksel dosya (SystemDocument)
        public SystemDocument? SystemDocument { get; set; }
    }
}
