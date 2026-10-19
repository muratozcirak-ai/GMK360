using System;
using System.Collections.Generic;

namespace GMK360.Web.Models
{
    public class TedarikciDashboardViewModel
    {
        public int TedarikciId { get; set; } = 1;
        public string StoreName { get; set; } = "Özkan Yapı Market & Elektrik Nalburiye";
        public string OwnerName { get; set; } = "Mustafa Özkan";
        public string PhoneNumber { get; set; } = "0533 111 22 33";
        public string CityName { get; set; } = "İstanbul";
        public string DistrictName { get; set; } = "Kadıköy";
        public string NeighborhoodName { get; set; } = "Fenerbahçe Mahallesi";
        public string Address { get; set; } = "Fenerbahçe Mah. Kalamış Cad. No: 42/A Kadıköy";

        // Hibrit Yetenek Matrisi
        public bool IsSeller { get; set; } = true; // Malzeme Satışı / Tedarik
        public bool IsSubcontractorToo { get; set; } = true; // Taşeronluk / Montaj da Yapıyor (Hibrit Usta/Esnaf)

        // Mahalle Freemium
        public bool IsNeighborhoodFreemium { get; set; } = true;

        // Kasa & Açık Hesap Özeti
        public decimal TotalOpenAccountReceivables { get; set; } // Toplam Açık Hesap Alacak (Ustalardan ve Müşterilerden)
        public decimal TotalCollectedThisMonth { get; set; } // Bu Ay Tahsil Edilen
        public int TotalActiveCustomersCount => Customers.Count;
        public int TotalOrganicCatalogItemsCount => OrganicCatalog.Count;

        // Müşteri / Usta Açık Hesap Listesi (Merkezi Tekil Kimlik ile)
        public List<TedarikciCustomerAccountViewModel> Customers { get; set; } = new List<TedarikciCustomerAccountViewModel>();

        // Organik Envanter Kütüphanesi (Yazarken Öğrenen Akıllı Liste)
        public List<TedarikciOrganicItemViewModel> OrganicCatalog { get; set; } = new List<TedarikciOrganicItemViewModel>();

        // "Ustan Var Mı?" Anlaşmalı ve Güvenilir Usta Rehberi
        public List<TedarikciTrustedUstaViewModel> TrustedUstas { get; set; } = new List<TedarikciTrustedUstaViewModel>();

        // Son Cari Hareketler
        public List<TedarikciTransactionViewModel> RecentTransactions { get; set; } = new List<TedarikciTransactionViewModel>();
    }

    public class TedarikciCustomerAccountViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string CustomerType { get; set; } = "Usta"; // "Usta", "Bina Yöneticisi", "Bireysel Ev Sahibi", "Müteahhit"
        public decimal CurrentDebtBalance { get; set; } // Nalburun bu kişiden toplam alacağı
        public DateTime LastTransactionDate { get; set; }
        public bool IsCentralVerified { get; set; } = true; // Merkezi havuzda tekilleşmiş mi?
        public string WhatsAppEkstreUrl { get; set; } = "";
    }

    public class TedarikciOrganicItemViewModel
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = null!; // "Dyo Dinamik Tavan Boyası 10 kg"
        public string Brand { get; set; } = "Dyo";
        public string Category { get; set; } = "Boya & Kimyasallar";
        public string Unit { get; set; } = "10 kg";
        public decimal? LastSoldPrice { get; set; } // 5000 TL
        public int TimesUsedCount { get; set; } = 1;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }

    public class TedarikciTrustedUstaViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string TradeRole { get; set; } = null!; // Elektrikçi, Boyacı, Su Tesisatçısı
        public string PhoneNumber { get; set; } = null!;
        public double Rating { get; set; } = 4.9;
        public string WhatsAppRecommendUrl { get; set; } = "";
    }

    public class TedarikciTransactionViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string RawTextEntered { get; set; } = null!; // "Dyo tavan boyası 10 kilo 5000"
        public string TransactionType { get; set; } = "Malzeme Verildi (+Borç)"; // Malzeme Verildi (+Borç) / Tahsilat Yapıldı (-Alacak)
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
    }
}
