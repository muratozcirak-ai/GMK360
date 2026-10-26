using System;

namespace GMK360.Core.DTOs.Construction
{
    public class ProjectPartnerDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string? CompanyName { get; set; } // Firma Ünvanı (Varsa)
        public string FirstName { get; set; } = ""; // Ortağın Adı
        public string LastName { get; set; } = ""; // Ortağın Soyadı
        public string? Phone { get; set; } // Telefon Numarası
        public string? Email { get; set; } // E-Posta Adresi
        public decimal SharePercentage { get; set; } = 0m; // Hisse Oranı (%)
        public string? ContractDocumentPath { get; set; } // Ortağın Sözleşmesi / Evrak Yolu
        public string? ContractDocumentName { get; set; } // Dosya Adı
        public string? Notes { get; set; } // Not / Açıklama

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(CompanyName) 
            ? FullName 
            : $"{CompanyName} ({FullName})";
    }
}
