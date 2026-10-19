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
    public interface IUstaWorkspaceService
    {
        Task<UstaDashboardViewModel> GetUstaDashboardAsync(int? ustaId = null);
        Task<UstaDashboardViewModel> ResetToSampleDataAsync();
        Task<UstaJobCardViewModel> CreateJobQuoteAsync(string jobTitle, string customerName, string customerPhone, string locationAddress, decimal agreedAmount, TaxShieldOption taxShield, bool isMaterialIncluded, string paymentMethod, string notes);
        Task<bool> UpdateJobStatusAsync(int jobId, string status);
        Task<bool> AddJobExpenseAsync(int jobId, string expenseType, decimal amount, string description);
        Task<bool> AddNalburTransactionAsync(string transactionType, decimal amount, string description, int? relatedJobId);
        Task<bool> AddAssistantAsync(string fullName, string phoneNumber, string role, decimal dailyWage);
        Task<bool> AddAssistantDailyWageAsync(int assistantId, int jobId, int daysCount);
        Task<bool> ToggleCityWideSubscriptionAsync();
        string GenerateWhatsAppMessage(UstaJobCardViewModel job, string ustaName, string ustaPhone, string ustaProfileUrl);
    }

    public class UstaWorkspaceService : IUstaWorkspaceService
    {
        private readonly IWebHostEnvironment _env;
        private static UstaDashboardViewModel? _cachedData;
        private static readonly object _lock = new();

        public UstaWorkspaceService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string GetStorageFilePath()
        {
            var dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            return Path.Combine(dataDir, "usta_workspace.json");
        }

        public async Task<UstaDashboardViewModel> GetUstaDashboardAsync(int? ustaId = null)
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
                        _cachedData = JsonSerializer.Deserialize<UstaDashboardViewModel>(json);
                        if (_cachedData != null)
                        {
                            RecalculateTotals(_cachedData);
                            return _cachedData;
                        }
                    }
                    catch
                    {
                        // JSON parsing hatasında sample veriye dön
                    }
                }

                _cachedData = BuildSampleUsta();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<UstaDashboardViewModel> ResetToSampleDataAsync()
        {
            lock (_lock)
            {
                _cachedData = BuildSampleUsta();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<UstaJobCardViewModel> CreateJobQuoteAsync(string jobTitle, string customerName, string customerPhone, string locationAddress, decimal agreedAmount, TaxShieldOption taxShield, bool isMaterialIncluded, string paymentMethod, string notes)
        {
            var dashboard = await GetUstaDashboardAsync();

            int nextId = dashboard.Jobs.Any() ? dashboard.Jobs.Max(j => j.Id) + 1 : 1;

            var job = new UstaJobCardViewModel
            {
                Id = nextId,
                JobTitle = jobTitle,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                LocationAddress = locationAddress,
                AgreedAmount = agreedAmount,
                TaxShield = taxShield,
                IsMaterialIncluded = isMaterialIncluded,
                PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "İş Bitimi Peşin / Havale" : paymentMethod,
                Notes = notes,
                Status = "Teklif Verildi",
                CreatedDate = DateTime.UtcNow,
                MaterialCost = 0,
                AssistantCost = 0,
                RoadFoodCost = 0
            };

            // Stopaj Kalkanı Rozeti ve Açıklaması
            ApplyTaxShieldBadges(job);

            // WhatsApp Metni Üret
            job.WhatsAppMessage = GenerateWhatsAppMessage(job, dashboard.FullName, dashboard.PhoneNumber, $"/usta/detay/{dashboard.UstaId}");
            job.WhatsAppUrl = BuildWhatsAppUrl(job.CustomerPhone, job.WhatsAppMessage);

            lock (_lock)
            {
                dashboard.Jobs.Insert(0, job);
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }

            return job;
        }

        public async Task<bool> UpdateJobStatusAsync(int jobId, string status)
        {
            var dashboard = await GetUstaDashboardAsync();
            var job = dashboard.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null) return false;

            lock (_lock)
            {
                job.Status = status;
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddJobExpenseAsync(int jobId, string expenseType, decimal amount, string description)
        {
            var dashboard = await GetUstaDashboardAsync();
            var job = dashboard.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (job == null) return false;

            lock (_lock)
            {
                if (expenseType.Equals("Material", StringComparison.OrdinalIgnoreCase))
                {
                    job.MaterialCost += amount;
                }
                else if (expenseType.Equals("Assistant", StringComparison.OrdinalIgnoreCase))
                {
                    job.AssistantCost += amount;
                }
                else
                {
                    job.RoadFoodCost += amount;
                }

                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddNalburTransactionAsync(string transactionType, decimal amount, string description, int? relatedJobId)
        {
            var dashboard = await GetUstaDashboardAsync();
            int nextId = dashboard.NalburTransactions.Any() ? dashboard.NalburTransactions.Max(t => t.Id) + 1 : 1;

            string? relatedJobTitle = null;
            if (relatedJobId.HasValue)
            {
                var relJob = dashboard.Jobs.FirstOrDefault(j => j.Id == relatedJobId.Value);
                if (relJob != null)
                {
                    relatedJobTitle = relJob.JobTitle;
                    // Eğer malzeme çekildiyse işin malzeme giderine de otomatik yansıt!
                    if (transactionType.Contains("Malzeme") || transactionType.Contains("Borç"))
                    {
                        relJob.MaterialCost += amount;
                    }
                }
            }

            decimal currentBalance = dashboard.NalburAccount.CurrentDebtBalance;
            bool isDebit = transactionType.Contains("Malzeme") || transactionType.Contains("Borç");
            decimal newBalance = isDebit ? currentBalance + amount : currentBalance - amount;
            if (newBalance < 0) newBalance = 0;

            var trans = new UstaNalburTransactionViewModel
            {
                Id = nextId,
                TransactionDate = DateTime.UtcNow,
                TransactionType = isDebit ? "Malzeme Çekildi (+Borç)" : "Ödeme Yapıldı (-Borç)",
                Amount = amount,
                Description = description,
                RelatedJobId = relatedJobId,
                RelatedJobTitle = relatedJobTitle,
                BalanceAfter = newBalance
            };

            lock (_lock)
            {
                dashboard.NalburAccount.CurrentDebtBalance = newBalance;
                dashboard.NalburTransactions.Insert(0, trans);
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddAssistantAsync(string fullName, string phoneNumber, string role, decimal dailyWage)
        {
            var dashboard = await GetUstaDashboardAsync();
            int nextId = dashboard.Assistants.Any() ? dashboard.Assistants.Max(a => a.Id) + 1 : 1;

            var assistant = new UstaAssistantViewModel
            {
                Id = nextId,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Role = string.IsNullOrWhiteSpace(role) ? "Çırak / Kalfa" : role,
                DailyWage = dailyWage > 0 ? dailyWage : 1200m,
                DaysWorkedTotal = 0,
                IsRegisteredFutureUsta = true,
                RegisteredDate = DateTime.UtcNow
            };

            lock (_lock)
            {
                dashboard.Assistants.Add(assistant);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddAssistantDailyWageAsync(int assistantId, int jobId, int daysCount)
        {
            var dashboard = await GetUstaDashboardAsync();
            var assistant = dashboard.Assistants.FirstOrDefault(a => a.Id == assistantId);
            var job = dashboard.Jobs.FirstOrDefault(j => j.Id == jobId);
            if (assistant == null || job == null || daysCount <= 0) return false;

            decimal totalWage = daysCount * assistant.DailyWage;

            lock (_lock)
            {
                assistant.DaysWorkedTotal += daysCount;
                job.AssistantCost += totalWage;
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> ToggleCityWideSubscriptionAsync()
        {
            var dashboard = await GetUstaDashboardAsync();
            lock (_lock)
            {
                dashboard.IsCityWideSubscriptionActive = !dashboard.IsCityWideSubscriptionActive;
                SaveToDisk(dashboard);
            }
            return dashboard.IsCityWideSubscriptionActive;
        }

        public string GenerateWhatsAppMessage(UstaJobCardViewModel job, string ustaName, string ustaPhone, string ustaProfileUrl)
        {
            string taxText = "";
            switch (job.TaxShield)
            {
                case TaxShieldOption.NetAgreed_TaxOnClient:
                    taxText = "🛡️ VERGİ & STOPAJ KALKANI:\nBu teklif 'NET ELE GEÇECEK TUTAR' olarak düzenlenmiştir. Apartman yönetimi veya işverenin düzenleyeceği gider pusulası stopajı, fon kesintileri ve yasal vergiler işverene aittir. Ustanın hak ettiği net tutardan 1 Kuruş dahi kesinti yapılamaz.";
                    break;
                case TaxShieldOption.WithholdingByClient:
                    taxText = "📄 GİDER PUSULASI / STOPAJ:\nİşveren gider pusulası düzenleyerek yasal vergiyi kendi vergi dairesine ödeyecektir. Ustanın eline tam net hakediş bedeli geçecektir.";
                    break;
                case TaxShieldOption.InvoiceWithVat:
                    taxText = "🧾 RESMİ FATURALI İŞLEM:\nTeklife %20 KDV dahil olup resmi e-fatura kesilecektir.";
                    break;
            }

            string materialText = job.IsMaterialIncluded
                ? "📦 Malzeme: Fiyata DAHİLDİR (Usta tarafından temin edilir)."
                : "📦 Malzeme: HARİÇTİR (Nalburdan çekilecek veya müşteri temin edecektir).";

            return $"Sayın {job.CustomerName},\n" +
                   $"GMK360 Usta Mutabakat Metnimizdir:\n\n" +
                   $"🛠️ Yapılacak İş: {job.JobTitle}\n" +
                   $"📍 Konum: {job.LocationAddress}\n" +
                   $"💰 Ustanın Net Hakedişi: {job.AgreedAmount:N0} TL\n\n" +
                   $"{taxText}\n\n" +
                   $"{materialText}\n" +
                   $"💳 Ödeme Şekli: {job.PaymentMethod}\n\n" +
                   $"👨‍🔧 Usta: {ustaName} ({ustaPhone})\n" +
                   $"🏅 MYK Belgeli & Onaylı Dijital Kartvizit: https://gmk360.com{ustaProfileUrl}\n\n" +
                   $"İş başlangıcı için bu mesajı 'ONAYLIYORUM' şeklinde yanıtlamanızı rica ederiz.";
        }

        private string BuildWhatsAppUrl(string phone, string message)
        {
            var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
            if (cleanPhone.StartsWith("0"))
            {
                cleanPhone = "90" + cleanPhone.Substring(1);
            }
            else if (!cleanPhone.StartsWith("90") && cleanPhone.Length == 10)
            {
                cleanPhone = "90" + cleanPhone;
            }

            var encoded = Uri.EscapeDataString(message);
            return $"https://api.whatsapp.com/send?phone={cleanPhone}&text={encoded}";
        }

        private void ApplyTaxShieldBadges(UstaJobCardViewModel job)
        {
            switch (job.TaxShield)
            {
                case TaxShieldOption.NetAgreed_TaxOnClient:
                    job.TaxShieldBadgeText = "🛡️ NET ANLAŞMA (Stopaj & Vergiler Yönetime Aittir)";
                    job.TaxShieldDescription = "Apartman yönetimi iş bitiminde 'stopaj kestik' diyerek usta hakedişinden kesinti yapamaz.";
                    break;
                case TaxShieldOption.WithholdingByClient:
                    job.TaxShieldBadgeText = "📄 STOPAJ MÜŞTERİDEN (Gider Pusulası Vergisi İşverene Ait)";
                    job.TaxShieldDescription = "Müşteri stopajı kendi öder, ustanın eline tam net tutar geçer.";
                    break;
                case TaxShieldOption.InvoiceWithVat:
                    job.TaxShieldBadgeText = "🧾 FATURALI İŞLEM (+%20 KDV)";
                    job.TaxShieldDescription = "Şirket üzerinden resmi fatura kesilir.";
                    break;
            }
        }

        private void RecalculateTotals(UstaDashboardViewModel vm)
        {
            vm.ActiveJobsCount = vm.Jobs.Count(j => j.Status == "Devam Ediyor" || j.Status == "Teklif Verildi");
            vm.CompletedJobsCount = vm.Jobs.Count(j => j.Status == "Tamamlandı" || j.Status == "Ödeme Alındı");

            vm.TotalAgreedRevenue = vm.Jobs.Sum(j => j.AgreedAmount);
            vm.TotalMaterialExpense = vm.Jobs.Sum(j => j.MaterialCost);
            vm.TotalAssistantExpense = vm.Jobs.Sum(j => j.AssistantCost);
            vm.TotalRoadFoodExpense = vm.Jobs.Sum(j => j.RoadFoodCost);

            vm.TotalNetProfit = vm.TotalAgreedRevenue - (vm.TotalMaterialExpense + vm.TotalAssistantExpense + vm.TotalRoadFoodExpense);

            foreach (var job in vm.Jobs)
            {
                ApplyTaxShieldBadges(job);
                job.WhatsAppMessage = GenerateWhatsAppMessage(job, vm.FullName, vm.PhoneNumber, $"/usta/detay/{vm.UstaId}");
                job.WhatsAppUrl = BuildWhatsAppUrl(job.CustomerPhone, job.WhatsAppMessage);
            }
        }

        private void SaveToDisk(UstaDashboardViewModel vm)
        {
            try
            {
                var path = GetStorageFilePath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(vm, options);
                File.WriteAllText(path, json);
            }
            catch
            {
                // Sessizce geç
            }
        }

        private UstaDashboardViewModel BuildSampleUsta()
        {
            var vm = new UstaDashboardViewModel
            {
                UstaId = 1,
                FullName = "Ahmet Usta (Akdoğan)",
                TradeTitle = "Elektrik & Sıhhi Tesisat Uzmanı",
                PhoneNumber = "0532 999 88 77",
                CityName = "İstanbul",
                DistrictName = "Kadıköy",
                NeighborhoodName = "Fenerbahçe Mahallesi",
                Bio = "20 yılı aşkın tecrübe ile Kadıköy ve çevresinde konut, iş yeri ve apartman elektrik panosu revizyonu, aydınlatma, akıllı röleler ve acil tesisat çözümleri.",
                IsVerified = true,
                ReliabilityScore = 98,
                AverageRating = 4.9,
                TotalReviewsCount = 48,
                IsSampleData = true,
                IsMahalleFreemiumActive = true,
                IsCityWideSubscriptionActive = false,
                CityWideMonthlyFee = 250m,

                Documents = new List<UstaDocumentViewModel>
                {
                    new UstaDocumentViewModel
                    {
                        Title = "MYK Seviye 4 Elektrik Tesisatçısı Mesleki Yeterlilik Belgesi",
                        Issuer = "Mesleki Yeterlilik Kurumu (MYK)",
                        DocumentNumber = "MYK-2024-88412",
                        Year = "2024",
                        IsApproved = true
                    },
                    new UstaDocumentViewModel
                    {
                        Title = "MEB Elektrik Tesisatçılığı Ustalık Diploması",
                        Issuer = "Milli Eğitim Bakanlığı Çıraklık & Mesleki Eğitim",
                        DocumentNumber = "MEB-UST-4412",
                        Year = "2012",
                        IsApproved = true
                    },
                    new UstaDocumentViewModel
                    {
                        Title = "Kadıköy Esnaf ve Sanatkarlar Odası Sicil Kaydı",
                        Issuer = "Kadıköy Esnaf Odası",
                        DocumentNumber = "KDK-10492",
                        Year = "2015",
                        IsApproved = true
                    }
                },

                Portfolios = new List<UstaPortfolioViewModel>
                {
                    new UstaPortfolioViewModel
                    {
                        Title = "Fenerbahçe Kalamış Cad. 4 Daireli Villa Pano & Röle Yenileme",
                        Location = "Kadıköy / Fenerbahçe",
                        Description = "Eski sigortalar ve yangın koruma röleleri Schneider ve Siemens marka ekipmanlarla tamamen yenilendi.",
                        BeforeImageUrl = "/images/portfolio/pano_eski.jpg",
                        AfterImageUrl = "/images/portfolio/pano_yeni.jpg",
                        ClientReference = "Mimar Serdar Bey (0533 *** 12)",
                        IsVerifiedReference = true
                    },
                    new UstaPortfolioViewModel
                    {
                        Title = "Göztepe Soyak Sitesi B Blok Asansör & Hidrofor Güç Hattı",
                        Location = "Kadıköy / Göztepe",
                        Description = "Jeneratör transfer panosu ve 5x10mm² yanmaz kablo çekimi 1 günde tamamlandı.",
                        BeforeImageUrl = "/images/portfolio/kablo_eski.jpg",
                        AfterImageUrl = "/images/portfolio/kablo_yeni.jpg",
                        ClientReference = "Site Yöneticisi Haldun Bey (0532 *** 88)",
                        IsVerifiedReference = true
                    }
                },

                NalburAccount = new UstaNalburAccountViewModel
                {
                    Id = 1,
                    NalburName = "Özkan Yapı & Elektrik Nalburiye",
                    ContactPerson = "Mustafa Özkan",
                    PhoneNumber = "0533 111 22 33",
                    District = "Kadıköy / Fenerbahçe",
                    CurrentDebtBalance = 4800m,
                    Status = "Açık Hesap (Veresiye) Aktif"
                },

                NalburTransactions = new List<UstaNalburTransactionViewModel>
                {
                    new UstaNalburTransactionViewModel
                    {
                        Id = 1,
                        TransactionDate = DateTime.UtcNow.AddDays(-2),
                        TransactionType = "Malzeme Çekildi (+Borç)",
                        Amount = 3200m,
                        Description = "Orkide Apt. Daire 8 Panosu için: 100m Hes NHXMH 3x2.5 Kablo + 4 adet Siemens V-Otomat Sigorta",
                        RelatedJobId = 1,
                        RelatedJobTitle = "Orkide Apt. Daire 8 - Pano & Aydınlatma Yenileme",
                        BalanceAfter = 6800m
                    },
                    new UstaNalburTransactionViewModel
                    {
                        Id = 2,
                        TransactionDate = DateTime.UtcNow.AddDays(-5),
                        TransactionType = "Ödeme Yapıldı (-Borç)",
                        Amount = 2000m,
                        Description = "Müşteriden hakediş avansı tahsil edildi, nalbura elden nakit ödendi.",
                        BalanceAfter = 3600m
                    },
                    new UstaNalburTransactionViewModel
                    {
                        Id = 3,
                        TransactionDate = DateTime.UtcNow.AddDays(-7),
                        TransactionType = "Malzeme Çekildi (+Borç)",
                        Amount = 1200m,
                        Description = "Moda işi için 3 kutu buat klemensi, buat kapakları ve izolasyon bantları",
                        RelatedJobId = 2,
                        RelatedJobTitle = "Moda Sahil Sitesi - Bahçe Aydınlatma Revizyonu",
                        BalanceAfter = 5600m
                    }
                },

                Assistants = new List<UstaAssistantViewModel>
                {
                    new UstaAssistantViewModel
                    {
                        Id = 1,
                        FullName = "Ali Çetin",
                        PhoneNumber = "0544 333 44 55",
                        Role = "Elektrik Kalfası",
                        DailyWage = 1200m,
                        DaysWorkedTotal = 2,
                        IsRegisteredFutureUsta = true,
                        RegisteredDate = DateTime.UtcNow.AddMonths(-3)
                    }
                },

                MaterialCatalog = new List<UstaMaterialPriceItemViewModel>
                {
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "Hes 3x2.5mm² NYM Antigron Kablo (100m)",
                        Category = "Elektrik & Kablo",
                        Unit = "Makara (100m)",
                        AveragePrice = 2150m,
                        SupplierName = "Özkan Yapı & Elektrik Toptan",
                        Location = "Kadıköy"
                    },
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "Siemens 1x16A B Tipi Otomat Sigorta (5SL6)",
                        Category = "Elektrik Pano",
                        Unit = "Adet",
                        AveragePrice = 145m,
                        SupplierName = "Özkan Yapı & Elektrik Toptan",
                        Location = "Kadıköy"
                    },
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "Siemens 40A 30mA Kaçak Akım Koruma Rölesi",
                        Category = "Elektrik Pano",
                        Unit = "Adet",
                        AveragePrice = 850m,
                        SupplierName = "Özkan Yapı & Elektrik Toptan",
                        Location = "Kadıköy"
                    },
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "Viko Karre Beyaz Topraklı Priz",
                        Category = "Aydınlatma & Anahtar",
                        Unit = "Adet",
                        AveragePrice = 68m,
                        SupplierName = "Özkan Yapı Market",
                        Location = "Kadıköy"
                    },
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "DYO Dinamik Silikonlu Mat İç Cephe Boyası (15L)",
                        Category = "Boya & Kimyasal",
                        Unit = "Teneke (15L)",
                        AveragePrice = 1850m,
                        SupplierName = "Bostancı Yapı Malzemeleri",
                        Location = "Kadıköy"
                    },
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "Fırat Kompozit PPRC Sıcak Su Borusu 25mm (4m)",
                        Category = "Sıhhi Tesisat",
                        Unit = "Boy (4m)",
                        AveragePrice = 120m,
                        SupplierName = "Marmara Tesisat Toptan",
                        Location = "Ataşehir"
                    },
                    new UstaMaterialPriceItemViewModel
                    {
                        MaterialName = "Kalekim 1051 Seramik & Fayans Yapıştırıcısı (25kg)",
                        Category = "İnşaat Harç & Yapıştırıcı",
                        Unit = "Torba (25kg)",
                        AveragePrice = 220m,
                        SupplierName = "Kadıköy Yapı Market",
                        Location = "Kadıköy"
                    }
                },

                Jobs = new List<UstaJobCardViewModel>
                {
                    new UstaJobCardViewModel
                    {
                        Id = 1,
                        JobTitle = "Orkide Apt. Daire 8 - Pano & Aydınlatma Yenileme",
                        CustomerName = "Mehmet Bey (Apartman Yöneticisi)",
                        CustomerPhone = "0532 555 12 34",
                        LocationAddress = "Kadıköy / Fenerbahçe Mah. Orkide Sok. No: 8 Daire 8",
                        AgreedAmount = 12000m,
                        TaxShield = TaxShieldOption.NetAgreed_TaxOnClient,
                        TaxShieldBadgeText = "🛡️ NET ANLAŞMA (Stopaj & Vergiler Yönetime Aittir)",
                        TaxShieldDescription = "Bu tutar ustanın cebine geçecek net tutardır. Yönetimin keseceği gider pusulası stopajı hakedişten düşülemez.",
                        IsMaterialIncluded = false,
                        PaymentMethod = "İş Bitimi Peşin / Havale",
                        Status = "Devam Ediyor",
                        CreatedDate = DateTime.UtcNow.AddDays(-2),
                        MaterialCost = 3200m,
                        AssistantCost = 2400m,
                        RoadFoodCost = 600m,
                        Notes = "Dairenin ana panosu değişecek, sigortalar B tipi Siemens takılacak. Salon tavan aydınlatma sortileri çekilecek."
                    },
                    new UstaJobCardViewModel
                    {
                        Id = 2,
                        JobTitle = "Moda Sahil Sitesi - Bahçe Aydınlatma Revizyonu",
                        CustomerName = "Ayşe Hanım (Site Denetçisi)",
                        CustomerPhone = "0533 444 55 66",
                        LocationAddress = "Kadıköy / Caferağa Mah. Moda Sahil Yolu No: 14",
                        AgreedAmount = 8500m,
                        TaxShield = TaxShieldOption.NetAgreed_TaxOnClient,
                        TaxShieldBadgeText = "🛡️ NET ANLAŞMA (Stopaj & Vergiler Yönetime Aittir)",
                        TaxShieldDescription = "Site yönetiminin gider pusulası stopajı yönetime aittir.",
                        IsMaterialIncluded = false,
                        PaymentMethod = "%50 Peşin + %50 İş Bitimi",
                        Status = "Teklif Verildi",
                        CreatedDate = DateTime.UtcNow.AddDays(-1),
                        MaterialCost = 800m,
                        AssistantCost = 0,
                        RoadFoodCost = 300m,
                        Notes = "Bahçe yürüyüş yolu spot aydınlatmalarındaki kaçak akım arızası giderilecek."
                    }
                }
            };

            RecalculateTotals(vm);
            return vm;
        }
    }
}
