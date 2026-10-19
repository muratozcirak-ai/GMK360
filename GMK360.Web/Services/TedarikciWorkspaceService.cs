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
    public interface ITedarikciWorkspaceService
    {
        Task<TedarikciDashboardViewModel> GetTedarikciDashboardAsync();
        Task<TedarikciDashboardViewModel> ResetToSampleDataAsync();
        Task<bool> AddFastTransactionAsync(int customerId, string rawInput, decimal amount, string transactionType);
        Task<bool> AddCustomerAccountAsync(string fullName, string phoneNumber, string customerType, decimal initialDebt);
        Task<bool> ToggleHybridCapabilityAsync(bool isSeller, bool isSubcontractor);
        string GenerateWhatsAppEkstre(TedarikciCustomerAccountViewModel customer, string storeName, string storePhone);
        string GenerateUstaRecommendationWhatsApp(TedarikciTrustedUstaViewModel usta, string storeName);
    }

    public class TedarikciWorkspaceService : ITedarikciWorkspaceService
    {
        private readonly IWebHostEnvironment _env;
        private static TedarikciDashboardViewModel? _cachedData;
        private static readonly object _lock = new();

        public TedarikciWorkspaceService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string GetStorageFilePath()
        {
            var dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            return Path.Combine(dataDir, "tedarikci_workspace.json");
        }

        public async Task<TedarikciDashboardViewModel> GetTedarikciDashboardAsync()
        {
            lock (_lock)
            {
                if (_cachedData != null)
                {
                    RecalculateTotals(_cachedData);
                    return _cachedData;
                }

                var path = GetStorageFilePath();
                if (File.Exists(path))
                {
                    try
                    {
                        var json = File.ReadAllText(path);
                        _cachedData = JsonSerializer.Deserialize<TedarikciDashboardViewModel>(json);
                        if (_cachedData != null)
                        {
                            RecalculateTotals(_cachedData);
                            return _cachedData;
                        }
                    }
                    catch { }
                }

                _cachedData = BuildSampleTedarikci();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<TedarikciDashboardViewModel> ResetToSampleDataAsync()
        {
            lock (_lock)
            {
                _cachedData = BuildSampleTedarikci();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<bool> AddFastTransactionAsync(int customerId, string rawInput, decimal amount, string transactionType)
        {
            var dashboard = await GetTedarikciDashboardAsync();
            var customer = dashboard.Customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null || amount <= 0 || string.IsNullOrWhiteSpace(rawInput)) return false;

            lock (_lock)
            {
                bool isDebit = transactionType.Contains("Malzeme") || transactionType.Contains("Borç");
                decimal newBalance = isDebit ? customer.CurrentDebtBalance + amount : customer.CurrentDebtBalance - amount;
                if (newBalance < 0) newBalance = 0;

                customer.CurrentDebtBalance = newBalance;
                customer.LastTransactionDate = DateTime.UtcNow;

                int transId = dashboard.RecentTransactions.Any() ? dashboard.RecentTransactions.Max(t => t.Id) + 1 : 1;
                dashboard.RecentTransactions.Insert(0, new TedarikciTransactionViewModel
                {
                    Id = transId,
                    Date = DateTime.UtcNow,
                    CustomerId = customerId,
                    CustomerName = customer.FullName,
                    RawTextEntered = rawInput,
                    TransactionType = isDebit ? "Malzeme Verildi (+Borç)" : "Tahsilat Yapıldı (-Alacak)",
                    Amount = amount,
                    BalanceAfter = newBalance
                });

                // AKILLI ORGANİK ENVANTER ÖĞRENME MOTORU
                LearnItemFromRawText(dashboard, rawInput, amount);

                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddCustomerAccountAsync(string fullName, string phoneNumber, string customerType, decimal initialDebt)
        {
            var dashboard = await GetTedarikciDashboardAsync();
            lock (_lock)
            {
                var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
                // MERKEZİ TEKİLLİK / DEDUPLICATION KONTROLÜ
                var existing = dashboard.Customers.FirstOrDefault(c => new string(c.PhoneNumber.Where(char.IsDigit).ToArray()) == cleanPhone);
                if (existing != null)
                {
                    // Zaten var, borç ekle ve güncelle
                    existing.CurrentDebtBalance += initialDebt;
                    existing.FullName = fullName;
                }
                else
                {
                    int nextId = dashboard.Customers.Any() ? dashboard.Customers.Max(c => c.Id) + 1 : 1;
                    dashboard.Customers.Insert(0, new TedarikciCustomerAccountViewModel
                    {
                        Id = nextId,
                        FullName = fullName,
                        PhoneNumber = phoneNumber,
                        CustomerType = customerType ?? "Usta",
                        CurrentDebtBalance = initialDebt,
                        LastTransactionDate = DateTime.UtcNow,
                        IsCentralVerified = true
                    });
                }

                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> ToggleHybridCapabilityAsync(bool isSeller, bool isSubcontractor)
        {
            var dashboard = await GetTedarikciDashboardAsync();
            lock (_lock)
            {
                dashboard.IsSeller = isSeller;
                dashboard.IsSubcontractorToo = isSubcontractor;
                SaveToDisk(dashboard);
            }
            return true;
        }

        public string GenerateWhatsAppEkstre(TedarikciCustomerAccountViewModel customer, string storeName, string storePhone)
        {
            var msg = $"Sayın {customer.FullName},\n" +
                      $"{storeName} güncel açık hesap (veresiye) ekstrenizdir:\n\n" +
                      $"💰 Güncel Kalan Bakiye Borcunuz: {customer.CurrentDebtBalance:N0} TL\n" +
                      $"📅 Son İşlem Tarihi: {customer.LastTransactionDate:dd.MM.yyyy}\n\n" +
                      $"Hayırlı işler dileriz.\n" +
                      $"İletişim: {storePhone}";

            var cleanPhone = new string(customer.PhoneNumber.Where(char.IsDigit).ToArray());
            if (cleanPhone.StartsWith("0")) cleanPhone = "90" + cleanPhone.Substring(1);
            else if (!cleanPhone.StartsWith("90") && cleanPhone.Length == 10) cleanPhone = "90" + cleanPhone;

            return $"https://api.whatsapp.com/send?phone={cleanPhone}&text={Uri.EscapeDataString(msg)}";
        }

        public string GenerateUstaRecommendationWhatsApp(TedarikciTrustedUstaViewModel usta, string storeName)
        {
            var msg = $"Merhaba,\n" +
                      $"{storeName} olarak eviniz/iş yeriniz için güvendiğimiz ustamızı öneriyoruz:\n\n" +
                      $"👨‍🔧 Usta: {usta.FullName}\n" +
                      $"🛠️ Uzmanlık: {usta.TradeRole}\n" +
                      $"📞 İletişim: {usta.PhoneNumber}\n" +
                      $"⭐ Müşteri Memnuniyeti: {usta.Rating} / 5.0\n\n" +
                      $"Gönül rahatlığıyla arayabilirsiniz.";

            return $"https://api.whatsapp.com/send?text={Uri.EscapeDataString(msg)}";
        }

        private void LearnItemFromRawText(TedarikciDashboardViewModel vm, string rawInput, decimal price)
        {
            var cleaned = rawInput.Trim();
            // Basit zekâ: Eğer bu ürün adı zaten organik kütüphanede yoksa ekle
            var existing = vm.OrganicCatalog.FirstOrDefault(i => cleaned.Contains(i.ItemName, StringComparison.OrdinalIgnoreCase) || i.ItemName.Contains(cleaned, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.TimesUsedCount++;
                existing.LastSoldPrice = price;
            }
            else
            {
                int nextId = vm.OrganicCatalog.Any() ? vm.OrganicCatalog.Max(i => i.Id) + 1 : 1;
                string brand = "Genel";
                string category = "Hırdavat & Nalbur";

                if (cleaned.Contains("dyo", StringComparison.OrdinalIgnoreCase) || cleaned.Contains("filli", StringComparison.OrdinalIgnoreCase) || cleaned.Contains("boya", StringComparison.OrdinalIgnoreCase))
                {
                    brand = cleaned.Contains("dyo", StringComparison.OrdinalIgnoreCase) ? "Dyo" : (cleaned.Contains("filli", StringComparison.OrdinalIgnoreCase) ? "Filli Boya" : "Yerel");
                    category = "Boya & Kimyasallar";
                }
                else if (cleaned.Contains("kablo", StringComparison.OrdinalIgnoreCase) || cleaned.Contains("sigorta", StringComparison.OrdinalIgnoreCase) || cleaned.Contains("priz", StringComparison.OrdinalIgnoreCase))
                {
                    category = "Elektrik Malzemeleri";
                }
                else if (cleaned.Contains("çivi", StringComparison.OrdinalIgnoreCase) || cleaned.Contains("vida", StringComparison.OrdinalIgnoreCase) || cleaned.Contains("tel", StringComparison.OrdinalIgnoreCase))
                {
                    category = "İnşaat Sarf & Hırdavat";
                }

                vm.OrganicCatalog.Add(new TedarikciOrganicItemViewModel
                {
                    Id = nextId,
                    ItemName = cleaned,
                    Brand = brand,
                    Category = category,
                    Unit = "Birim",
                    LastSoldPrice = price,
                    TimesUsedCount = 1,
                    AddedDate = DateTime.UtcNow
                });
            }
        }

        private void RecalculateTotals(TedarikciDashboardViewModel vm)
        {
            vm.TotalOpenAccountReceivables = vm.Customers.Sum(c => c.CurrentDebtBalance);
            vm.TotalCollectedThisMonth = vm.RecentTransactions.Where(t => t.TransactionType.Contains("Tahsilat")).Sum(t => t.Amount);

            foreach (var c in vm.Customers)
            {
                c.WhatsAppEkstreUrl = GenerateWhatsAppEkstre(c, vm.StoreName, vm.PhoneNumber);
            }
            foreach (var u in vm.TrustedUstas)
            {
                u.WhatsAppRecommendUrl = GenerateUstaRecommendationWhatsApp(u, vm.StoreName);
            }
        }

        private void SaveToDisk(TedarikciDashboardViewModel vm)
        {
            try
            {
                var path = GetStorageFilePath();
                var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }
        }

        private TedarikciDashboardViewModel BuildSampleTedarikci()
        {
            var vm = new TedarikciDashboardViewModel
            {
                TedarikciId = 1,
                StoreName = "Özkan Yapı Market & Elektrik Nalburiye",
                OwnerName = "Mustafa Özkan",
                PhoneNumber = "0533 111 22 33",
                CityName = "İstanbul",
                DistrictName = "Kadıköy",
                NeighborhoodName = "Fenerbahçe Mahallesi",
                Address = "Fenerbahçe Mah. Kalamış Cad. No: 42/A Kadıköy",
                IsSeller = true,
                IsSubcontractorToo = true, // Hem satıcı hem montaj/taşeronluk yapıyor!
                IsNeighborhoodFreemium = true,

                Customers = new List<TedarikciCustomerAccountViewModel>
                {
                    new TedarikciCustomerAccountViewModel
                    {
                        Id = 1,
                        FullName = "Ahmet Usta (Elektrikçi)",
                        PhoneNumber = "0532 999 88 77",
                        CustomerType = "Usta",
                        CurrentDebtBalance = 4800m,
                        LastTransactionDate = DateTime.UtcNow.AddDays(-2),
                        IsCentralVerified = true
                    },
                    new TedarikciCustomerAccountViewModel
                    {
                        Id = 2,
                        FullName = "Hasan Kaya (Kalıp-Demir Taşeronu)",
                        PhoneNumber = "0533 888 77 66",
                        CustomerType = "Taşeron",
                        CurrentDebtBalance = 8500m,
                        LastTransactionDate = DateTime.UtcNow.AddDays(-8),
                        IsCentralVerified = true
                    },
                    new TedarikciCustomerAccountViewModel
                    {
                        Id = 3,
                        FullName = "Orkide Apt. Yöneticisi Mehmet Bey",
                        PhoneNumber = "0532 555 12 34",
                        CustomerType = "Bina Yöneticisi",
                        CurrentDebtBalance = 1650m,
                        LastTransactionDate = DateTime.UtcNow.AddDays(-4),
                        IsCentralVerified = true
                    }
                },

                OrganicCatalog = new List<TedarikciOrganicItemViewModel>
                {
                    new TedarikciOrganicItemViewModel
                    {
                        Id = 1,
                        ItemName = "Dyo Dinamik Tavan Boyası 10 kg",
                        Brand = "Dyo",
                        Category = "Boya & Kimyasallar",
                        Unit = "10 kg Teneke",
                        LastSoldPrice = 5000m,
                        TimesUsedCount = 14
                    },
                    new TedarikciOrganicItemViewModel
                    {
                        Id = 2,
                        ItemName = "5'lik İnşaat Çivisi 1 kg",
                        Brand = "Genel",
                        Category = "İnşaat Sarf & Hırdavat",
                        Unit = "1 kg Paket",
                        LastSoldPrice = 85m,
                        TimesUsedCount = 28
                    },
                    new TedarikciOrganicItemViewModel
                    {
                        Id = 3,
                        ItemName = "Hes 3x2.5mm² NYM Antigron Kablo (100m)",
                        Brand = "Hes",
                        Category = "Elektrik Malzemeleri",
                        Unit = "100m Makara",
                        LastSoldPrice = 2150m,
                        TimesUsedCount = 9
                    },
                    new TedarikciOrganicItemViewModel
                    {
                        Id = 4,
                        ItemName = "Filli Boya Betakril Beyaz 15L",
                        Brand = "Filli Boya",
                        Category = "Boya & Kimyasallar",
                        Unit = "15L Teneke",
                        LastSoldPrice = 1950m,
                        TimesUsedCount = 8
                    }
                },

                TrustedUstas = new List<TedarikciTrustedUstaViewModel>
                {
                    new TedarikciTrustedUstaViewModel
                    {
                        Id = 1,
                        FullName = "Ahmet Usta (Elektrik & Tesisat)",
                        TradeRole = "Elektrik & Sıhhi Tesisat Uzmanı",
                        PhoneNumber = "0532 999 88 77",
                        Rating = 4.9
                    },
                    new TedarikciTrustedUstaViewModel
                    {
                        Id = 2,
                        FullName = "Murat Usta (Boya & Badana)",
                        TradeRole = "İç & Dış Cephe Boya Ustası",
                        PhoneNumber = "0535 333 22 11",
                        Rating = 4.8
                    }
                },

                RecentTransactions = new List<TedarikciTransactionViewModel>
                {
                    new TedarikciTransactionViewModel
                    {
                        Id = 1,
                        Date = DateTime.UtcNow.AddDays(-2),
                        CustomerId = 1,
                        CustomerName = "Ahmet Usta (Elektrikçi)",
                        RawTextEntered = "100m Hes kablo + 4 adet sigorta 3200",
                        TransactionType = "Malzeme Verildi (+Borç)",
                        Amount = 3200m,
                        BalanceAfter = 4800m
                    },
                    new TedarikciTransactionViewModel
                    {
                        Id = 2,
                        Date = DateTime.UtcNow.AddDays(-5),
                        CustomerId = 1,
                        CustomerName = "Ahmet Usta (Elektrikçi)",
                        RawTextEntered = "Ahmet Usta hakedişten elden ödedi",
                        TransactionType = "Tahsilat Yapıldı (-Alacak)",
                        Amount = 2000m,
                        BalanceAfter = 1600m
                    }
                }
            };

            RecalculateTotals(vm);
            return vm;
        }
    }
}
