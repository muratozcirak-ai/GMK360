using System;
using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class KurumsalB2BDashboardViewModel
    {
        public int TotalVendorsCount => Suppliers.Count + TechnicalAuthorities.Count + LegalAdvisors.Count;
        
        // 1. İhtisaslaşmış Endüstriyel Malzeme & Taşeron Distribütörleri
        public List<KurumsalSupplierCardViewModel> Suppliers { get; set; } = new List<KurumsalSupplierCardViewModel>();

        // 2. Bakanlık Lisanslı Teknik Otoriteler & Laboratuvarlar
        public List<KurumsalTechnicalAuthorityViewModel> TechnicalAuthorities { get; set; } = new List<KurumsalTechnicalAuthorityViewModel>();

        // 3. Gayrimenkul, İmar & Şantiye SGK Hukuk Masası
        public List<KurumsalLegalAdvisorViewModel> LegalAdvisors { get; set; } = new List<KurumsalLegalAdvisorViewModel>();

        // Şantiye DNA İhale Talepleri (Müteahhitlerden Gelen Doğrudan İhaleler)
        public List<KurumsalDnaTenderRequestViewModel> ActiveDnaTenders { get; set; } = new List<KurumsalDnaTenderRequestViewModel>();
    }

    public class KurumsalSupplierCardViewModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = null!;
        public string SpecializationCategory { get; set; } = null!; // "Orman Ürünleri (Plywood & Kereste)", "Kuru Duvar & Alçıpan", "Hazır Beton Santrali", "Çatı Sistemleri (Braas vb.)", "Demir & Çelik"
        public string CityDistrict { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool SuppliesMaterial { get; set; } = true;
        public bool ProvidesLaborInstallation { get; set; } = false; // Taşeronluk / Montaj da yapıyor mu?
        public string MinimumOrderScale { get; set; } = null!; // "Tır / Kamyon Bazlı", "Mikser / Pompa", "Palet"
        public List<string> MainProducts { get; set; } = new List<string>();
        public double Rating { get; set; } = 4.9;
        public string WhatsAppQuoteUrl { get; set; } = "";
    }

    public class KurumsalTechnicalAuthorityViewModel
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = null!;
        public string AuthorityType { get; set; } = null!; // "Yapı Denetim Kuruluşu (Bakanlık Lisanslı)", "Akredite Test Laboratuvarı (TÜRKAK)", "LİHKAB Harita Mühendisliği"
        public string LicenseNumber { get; set; } = null!;
        public string CityDistrict { get; set; } = null!;
        public string ContactPerson { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public List<string> ServicesProvided { get; set; } = new List<string>();
        public bool IsMinistryAccredited { get; set; } = true;
    }

    public class KurumsalLegalAdvisorViewModel
    {
        public int Id { get; set; }
        public string LawyerOrFirmName { get; set; } = null!;
        public string PracticeArea { get; set; } = null!; // "Kentsel Dönüşüm & Kat Karşılığı Hukuku", "KMK & Apartman Yönetimi İcra", "Şantiye SGK Asgari İşçilik Danışmanlığı"
        public string BarAssociationCity { get; set; } = "İstanbul Barosu";
        public string ContactPhone { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<string> KeySpecialties { get; set; } = new List<string>();
    }

    public class KurumsalDnaTenderRequestViewModel
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = null!;
        public string ContractorCompany { get; set; } = null!;
        public string TenderCategory { get; set; } = null!; // "Plywood & Kalıp Kerestesi", "Hazır Beton C30/35", "Alçıpan & Profil"
        public string QuantityDescription { get; set; } = null!; // "3 Tır 18mm Plywood + 80 m³ Çam Kereste"
        public DateTime DeadlineDate { get; set; }
        public string Location { get; set; } = null!;
    }
}
