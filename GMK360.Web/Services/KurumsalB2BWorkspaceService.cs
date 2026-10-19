using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GMK360.Web.Models;
using Microsoft.AspNetCore.Hosting;

namespace GMK360.Web.Services
{
    public interface IKurumsalB2BWorkspaceService
    {
        Task<KurumsalB2BDashboardViewModel> GetDashboardAsync();
        Task<KurumsalB2BDashboardViewModel> ResetToSampleDataAsync();
        Task<bool> SubmitDnaTenderBidAsync(int tenderId, string bidderCompany, decimal bidAmount, string notes);
    }

    public class KurumsalB2BWorkspaceService : IKurumsalB2BWorkspaceService
    {
        private readonly IWebHostEnvironment _env;
        private static KurumsalB2BDashboardViewModel? _cachedData;
        private static readonly object _lock = new();

        public KurumsalB2BWorkspaceService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string GetStorageFilePath()
        {
            var dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            return Path.Combine(dataDir, "kurumsal_b2b_workspace.json");
        }

        public async Task<KurumsalB2BDashboardViewModel> GetDashboardAsync()
        {
            lock (_lock)
            {
                if (_cachedData != null) return _cachedData;

                var path = GetStorageFilePath();
                if (File.Exists(path))
                {
                    try
                    {
                        var json = File.ReadAllText(path);
                        _cachedData = JsonSerializer.Deserialize<KurumsalB2BDashboardViewModel>(json);
                        if (_cachedData != null) return _cachedData;
                    }
                    catch { }
                }

                _cachedData = BuildSampleData();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<KurumsalB2BDashboardViewModel> ResetToSampleDataAsync()
        {
            lock (_lock)
            {
                _cachedData = BuildSampleData();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<bool> SubmitDnaTenderBidAsync(int tenderId, string bidderCompany, decimal bidAmount, string notes)
        {
            var dashboard = await GetDashboardAsync();
            var tender = dashboard.ActiveDnaTenders.FirstOrDefault(t => t.Id == tenderId);
            if (tender == null || bidAmount <= 0) return false;

            lock (_lock)
            {
                SaveToDisk(dashboard);
            }
            return true;
        }

        private void SaveToDisk(KurumsalB2BDashboardViewModel vm)
        {
            try
            {
                var path = GetStorageFilePath();
                var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }
        }

        private KurumsalB2BDashboardViewModel BuildSampleData()
        {
            return new KurumsalB2BDashboardViewModel
            {
                Suppliers = new List<KurumsalSupplierCardViewModel>
                {
                    new KurumsalSupplierCardViewModel
                    {
                        Id = 1,
                        CompanyName = "Marmara Ahşap & Orman Ürünleri Sanayi A.Ş.",
                        SpecializationCategory = "Orman Ürünleri (Plywood & Kereste)",
                        CityDistrict = "İstanbul / Ümraniye - Dudullu OSB",
                        ContactPerson = "Serdar Aktaş (Satış Müdürü)",
                        PhoneNumber = "0216 444 88 11",
                        SuppliesMaterial = true,
                        ProvidesLaborInstallation = false,
                        MinimumOrderScale = "Tır / Kamyon Bazlı Sevkiyat",
                        MainProducts = new List<string> { "18mm Huş (Birch) Plywood", "5x10 / 10x10 İnşaatlık Çam Kereste", "OSB-3 Levha", "Laminat Parke & Süpürgelik" },
                        Rating = 4.9,
                        WhatsAppQuoteUrl = "https://api.whatsapp.com/send?phone=905321112233"
                    },
                    new KurumsalSupplierCardViewModel
                    {
                        Id = 2,
                        CompanyName = "Ak Kuru Duvar & Alçıpan Toptan Dağıtım",
                        SpecializationCategory = "Kuru Duvar & Alçıpan",
                        CityDistrict = "İstanbul / Maltepe",
                        ContactPerson = "Cengiz Bey",
                        PhoneNumber = "0216 333 44 55",
                        SuppliesMaterial = true,
                        ProvidesLaborInstallation = true, // Hem toptan malzeme hem taşeronluk!
                        MinimumOrderScale = "Palet / Kamyon Bazlı",
                        MainProducts = new List<string> { "Beyaz Standart Alçıpan", "Yeşil Suya Dayanıklı Alçıpan", "Galvaniz C-U Duvar & Tavan Profilleri", "Akustik Asma Tavan" },
                        Rating = 4.8,
                        WhatsAppQuoteUrl = "https://api.whatsapp.com/send?phone=905332223344"
                    },
                    new KurumsalSupplierCardViewModel
                    {
                        Id = 3,
                        CompanyName = "Boğaziçi Hazır Beton Santralleri",
                        SpecializationCategory = "Hazır Beton Santrali",
                        CityDistrict = "İstanbul / Ataşehir & Kartal Santrali",
                        ContactPerson = "Mühendis Erhan Bey (Santral Şefi)",
                        PhoneNumber = "0216 555 77 88",
                        SuppliesMaterial = true,
                        ProvidesLaborInstallation = false,
                        MinimumOrderScale = "Transmikser & Mobil Pompa",
                        MainProducts = new List<string> { "C25/30 Hazır Beton", "C35/45 Yüksek Dayanımlı Beton", "Grobeton C16", "Şap Betonu" },
                        Rating = 5.0,
                        WhatsAppQuoteUrl = "https://api.whatsapp.com/send?phone=905345556677"
                    },
                    new KurumsalSupplierCardViewModel
                    {
                        Id = 4,
                        CompanyName = "Braas Sertifikalı Çatı Çözümleri & Uygulama",
                        SpecializationCategory = "Çatı Sistemleri (Braas)",
                        CityDistrict = "İstanbul / Kadıköy - Kozyatağı",
                        ContactPerson = "Mimar Hakan Güler",
                        PhoneNumber = "0216 666 99 00",
                        SuppliesMaterial = true,
                        ProvidesLaborInstallation = true, // Hibrit: Ürün + Anahtar Teslim Montaj + 30 Yıl Garanti
                        MinimumOrderScale = "Şantiye Proje Bazlı",
                        MainProducts = new List<string> { "Braas Kiremit & Mahya", "Nefes Alan Su Yalıtım Membranı", "Gizli Dere Yağmur İndirme", "30 Yıl Sistem Garantili İşçilik" },
                        Rating = 4.9,
                        WhatsAppQuoteUrl = "https://api.whatsapp.com/send?phone=905367778899"
                    }
                },

                TechnicalAuthorities = new List<KurumsalTechnicalAuthorityViewModel>
                {
                    new KurumsalTechnicalAuthorityViewModel
                    {
                        Id = 1,
                        OrganizationName = "Avrasya Yapı Denetim Kuruluşu",
                        AuthorityType = "Yapı Denetim Kuruluşu (Bakanlık İzinli)",
                        LicenseNumber = "YDK-1482",
                        CityDistrict = "İstanbul / Kadıköy",
                        ContactPerson = "İnş. Müh. Kenan Yıldırım",
                        PhoneNumber = "0216 418 20 30",
                        ServicesProvided = new List<string> { "Demir Donatı Kontrolü & Vizesi", "Kalıp & Aks Kontrolü", "Beton Döküm İzni", "Bina Hak Ediş Onayları (ÇŞB)" },
                        IsMinistryAccredited = true
                    },
                    new KurumsalTechnicalAuthorityViewModel
                    {
                        Id = 2,
                        OrganizationName = "Atlas Akredite Zemin & Beton Test Laboratuvarı",
                        AuthorityType = "Akredite Test Laboratuvarı (TÜRKAK)",
                        LicenseNumber = "TÜRKAK-AB-0482-T",
                        CityDistrict = "İstanbul / Ataşehir",
                        ContactPerson = "Jeoloji Müh. Burak Tekin",
                        PhoneNumber = "0216 574 11 22",
                        ServicesProvided = new List<string> { "7 ve 28 Günlük Küp Beton Basınç Kırma", "Karot Numune Alma & Demir Röntgeni", "Sismik Zemin Etüt Raporu", "Çelik Çekme Deneyi" },
                        IsMinistryAccredited = true
                    }
                },

                LegalAdvisors = new List<KurumsalLegalAdvisorViewModel>
                {
                    new KurumsalLegalAdvisorViewModel
                    {
                        Id = 1,
                        LawyerOrFirmName = "Av. Selim Karahan & Ortakları Hukuk Bürosu",
                        PracticeArea = "Kentsel Dönüşüm, İmar & İnşaat Hukuku",
                        BarAssociationCity = "İstanbul Barosu (Sicil: 24190)",
                        ContactPhone = "0216 345 80 90",
                        Description = "Arsa payı karşılığı inşaat sözleşmeleri (KKİS), kentsel dönüşüm 2/3 çoğunluk kararları, müteahhit teminatları ve tapu tescil davaları uzmanlığı.",
                        KeySpecialties = new List<string> { "Kat Karşılığı İnşaat Sözleşmeleri", "Kentsel Dönüşüm Tahliye & Sözleşme Feshi", "Şantiye SGK Asgari İşçilik ve İskân Danışmanlığı", "KMK Aidat İcra & Yönetim Danışmanlığı" }
                    }
                },

                ActiveDnaTenders = new List<KurumsalDnaTenderRequestViewModel>
                {
                    new KurumsalDnaTenderRequestViewModel
                    {
                        Id = 1,
                        ProjectName = "Beyaz Konak Rezidans (10 Kat)",
                        ContractorCompany = "Dengehan İnşaat A.Ş.",
                        TenderCategory = "Orman Ürünleri (Plywood & Kereste)",
                        QuantityDescription = "3 Tır 18mm Huş Plywood (1.500 Plaka) + 60 m³ Kalıp Kerestesi",
                        DeadlineDate = DateTime.UtcNow.AddDays(3),
                        Location = "Kadıköy / Fenerbahçe"
                    },
                    new KurumsalDnaTenderRequestViewModel
                    {
                        Id = 2,
                        ProjectName = "Ataşehir Park Evleri Şantiyesi",
                        ContractorCompany = "Örnek Yapı Sanayi Ltd.",
                        TenderCategory = "Hazır Beton Santrali",
                        QuantityDescription = "2.400 m³ C30/37 Hazır Beton (4 Kat Tabyası & Kolonlar Pompalı Döküm)",
                        DeadlineDate = DateTime.UtcNow.AddDays(5),
                        Location = "Ataşehir / Batı"
                    }
                }
            };
        }
    }
}
