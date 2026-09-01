using System;

namespace GMK360.Core.Entities
{
    public class ServiceProviderPortfolio : BaseEntity
    {
        public int ServiceProviderId { get; set; }
        public ServiceProvider ServiceProvider { get; set; }

        public string ProjectName { get; set; } // Örn: "Ataşehir 25 Villalık Kompleks Seramik İşi"
        public string Description { get; set; }
        public string ImageUrl { get; set; } // Biten işin fotoğrafı

                public DateTime CompletionDate { get; set; }
        
        // Referans Onay ve Viral Büyüme (Growth Hacking) Alanları
        public string? ReferenceName { get; set; } // Referans kişisinin adı (Örn: Müteahhit Yılmaz Bey)
        public string? ReferencePhone { get; set; } // SMS gidecek numara
        public string? VerificationToken { get; set; } // SMS linki için eşsiz kod
        public bool IsVerifiedReference { get; set; } = false; // Platform üzerinden mi alındı / SMS onaylandı mı?
    }
}
