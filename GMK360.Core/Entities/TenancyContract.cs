using System;

namespace GMK360.Core.Entities
{
    public class TenancyContract : BaseEntity
    {
        public int PropertyId { get; set; } // Hangi mülk (Dijital Evim içindeki)
        public Property Property { get; set; }

        public string TenantName { get; set; } // Kiracı Adı/Unvanı/TCKN
        public DateTime StartDate { get; set; } // Sözleşme Başlangıcı
        public DateTime EndDate { get; set; } // Sözleşme Bitişi
        public decimal MonthlyRentAmount { get; set; } // Güncel Kira Bedeli
        public int PaymentDay { get; set; } // Her ayın kaçıncı günü ödenecek?
                public string? PayeeName { get; set; } // Kime ödenecek (Ad Soyad)
        public string? PayeeIban { get; set; } // IBAN Numarası
        public bool UseIyzico { get; set; } // Iyzico güvencesi ile ödeme isteniyor mu?

        // Noter Onayı ve Belge Yönetimi
        public bool IsNotaryApproved { get; set; } = false; // Sözleşme noterde mi yapıldı?
        public string? NotaryName { get; set; } // Hangi Noter (Örn: Kadıköy 4. Noter)
        public string? NotaryNumber { get; set; } // Yevmiye Numarası

        public bool IsActive { get; set; } = true;
    }
}
