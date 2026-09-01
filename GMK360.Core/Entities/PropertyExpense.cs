using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GMK360.Core.Entities
{
    public enum PropertyExpenseCategory
    {
        MaintenanceRepair = 1, // Çatı akıntısı, boya badana, kombi tamiri
        Insurance = 2,         // DASK ve İsteğe Bağlı Konut Sigortası
        LoanInterest = 3,      // Banka kredisi faizleri
        ManagementFee = 4,     // Ev sahibinin ödediği aidat/yakıt vb.
        NewHomeDeduction = 5,  // Yeni Ev İndirimi (Satın alma bedelinin %5'i)
        RentPaidByOwner = 6,   // Kirada oturan ev sahibinin kendi kirası
        Other = 99
    }

    public class PropertyExpense : BaseEntity
    {
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }

        public PropertyExpenseCategory ExpenseCategory { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string? InvoiceDocumentUrl { get; set; } // Fatura veya Fiş Görseli

        public bool IsTaxDeductible { get; set; } = true; // Vergiden düşülebilir (Gerçek gider) mi? (Değer artırıcı ise false olabilir)

        // Hangi beyanname yılında kullanılacağı
        public int TaxYear { get; set; }
    }
}
