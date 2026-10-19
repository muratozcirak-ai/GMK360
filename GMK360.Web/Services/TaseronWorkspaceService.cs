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
    public interface ITaseronWorkspaceService
    {
        Task<TaseronDashboardViewModel> GetTaseronDashboardAsync();
        Task<TaseronDashboardViewModel> ResetToSampleDataAsync();
        Task<bool> RecordDailyPuantajAsync(DateTime date, int workerId, double workDayFactor, int overtimeHours, string siteLocation, string notes);
        Task<bool> BulkApproveAllWorkersTodayAsync(DateTime date, string siteLocation);
        Task<bool> AddWorkerAdvanceAsync(int workerId, decimal amount, string paymentType, string notes);
        Task<bool> AddWorkerAsync(string fullName, string phoneNumber, string tradeRole, decimal dailyWage);
        Task<bool> CreateContractHakedisClaimAsync(int contractId, decimal hakedisAmount, string progressNotes);
        Task<bool> AddEquipmentExpenseAsync(string itemName, string supplierName, decimal amount, string category);
        Task<bool> ToggleBranchAsync(string branchName);
        string GenerateContractWhatsAppSummary(TaseronContractViewModel contract, string taseronName, string taseronPhone);
    }

    public class TaseronWorkspaceService : ITaseronWorkspaceService
    {
        private readonly IWebHostEnvironment _env;
        private static TaseronDashboardViewModel? _cachedData;
        private static readonly object _lock = new();

        public TaseronWorkspaceService(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string GetStorageFilePath()
        {
            var dataDir = Path.Combine(_env.ContentRootPath, "App_Data");
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            return Path.Combine(dataDir, "taseron_workspace.json");
        }

        public async Task<TaseronDashboardViewModel> GetTaseronDashboardAsync()
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
                        _cachedData = JsonSerializer.Deserialize<TaseronDashboardViewModel>(json);
                        if (_cachedData != null)
                        {
                            RecalculateTotals(_cachedData);
                            return _cachedData;
                        }
                    }
                    catch { }
                }

                _cachedData = BuildSampleTaseron();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<TaseronDashboardViewModel> ResetToSampleDataAsync()
        {
            lock (_lock)
            {
                _cachedData = BuildSampleTaseron();
                SaveToDisk(_cachedData);
                return _cachedData;
            }
        }

        public async Task<bool> RecordDailyPuantajAsync(DateTime date, int workerId, double workDayFactor, int overtimeHours, string siteLocation, string notes)
        {
            var dashboard = await GetTaseronDashboardAsync();
            var worker = dashboard.Workers.FirstOrDefault(w => w.Id == workerId);
            if (worker == null) return false;

            lock (_lock)
            {
                int nextId = dashboard.RecentPuantajEntries.Any() ? dashboard.RecentPuantajEntries.Max(p => p.Id) + 1 : 1;
                var entry = new TaseronPuantajEntryViewModel
                {
                    Id = nextId,
                    Date = date,
                    WorkerId = workerId,
                    WorkerName = worker.FullName,
                    TradeRole = worker.TradeRole,
                    WorkDayFactor = workDayFactor,
                    OvertimeHours = overtimeHours,
                    SiteLocation = siteLocation,
                    Notes = notes
                };

                dashboard.RecentPuantajEntries.Insert(0, entry);
                worker.DaysWorkedThisMonth += workDayFactor;
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> BulkApproveAllWorkersTodayAsync(DateTime date, string siteLocation)
        {
            var dashboard = await GetTaseronDashboardAsync();
            lock (_lock)
            {
                foreach (var worker in dashboard.Workers)
                {
                    int nextId = dashboard.RecentPuantajEntries.Any() ? dashboard.RecentPuantajEntries.Max(p => p.Id) + 1 : 1;
                    dashboard.RecentPuantajEntries.Insert(0, new TaseronPuantajEntryViewModel
                    {
                        Id = nextId,
                        Date = date,
                        WorkerId = worker.Id,
                        WorkerName = worker.FullName,
                        TradeRole = worker.TradeRole,
                        WorkDayFactor = 1.0,
                        OvertimeHours = 0,
                        SiteLocation = siteLocation ?? "Şantiye Sahası",
                        Notes = "Toplu puantaj onayı ile tam gün kaydedildi."
                    });
                    worker.DaysWorkedThisMonth += 1.0;
                }
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddWorkerAdvanceAsync(int workerId, decimal amount, string paymentType, string notes)
        {
            var dashboard = await GetTaseronDashboardAsync();
            var worker = dashboard.Workers.FirstOrDefault(w => w.Id == workerId);
            if (worker == null || amount <= 0) return false;

            lock (_lock)
            {
                int nextId = dashboard.Advances.Any() ? dashboard.Advances.Max(a => a.Id) + 1 : 1;
                var advance = new TaseronAdvanceViewModel
                {
                    Id = nextId,
                    Date = DateTime.UtcNow,
                    WorkerId = workerId,
                    WorkerName = worker.FullName,
                    Amount = amount,
                    PaymentType = string.IsNullOrWhiteSpace(paymentType) ? "Elden Nakit Harcırah" : paymentType,
                    Notes = notes
                };

                dashboard.Advances.Insert(0, advance);
                worker.TotalAdvancesTaken += amount;
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddWorkerAsync(string fullName, string phoneNumber, string tradeRole, decimal dailyWage)
        {
            var dashboard = await GetTaseronDashboardAsync();
            lock (_lock)
            {
                int nextId = dashboard.Workers.Any() ? dashboard.Workers.Max(w => w.Id) + 1 : 1;
                dashboard.Workers.Add(new TaseronWorkerViewModel
                {
                    Id = nextId,
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    TradeRole = string.IsNullOrWhiteSpace(tradeRole) ? "İnşaat Ustası" : tradeRole,
                    DailyWage = dailyWage > 0 ? dailyWage : 1500m,
                    DaysWorkedThisMonth = 0,
                    TotalAdvancesTaken = 0,
                    IsRegisteredFutureCraftsman = true
                });
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> CreateContractHakedisClaimAsync(int contractId, decimal hakedisAmount, string progressNotes)
        {
            var dashboard = await GetTaseronDashboardAsync();
            var contract = dashboard.Contracts.FirstOrDefault(c => c.Id == contractId);
            if (contract == null || hakedisAmount <= 0) return false;

            lock (_lock)
            {
                contract.CumulativeHakedisPaid += contract.CurrentPeriodHakedisClaim;
                contract.CurrentPeriodHakedisClaim = hakedisAmount;
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> AddEquipmentExpenseAsync(string itemName, string supplierName, decimal amount, string category)
        {
            var dashboard = await GetTaseronDashboardAsync();
            if (amount <= 0) return false;

            lock (_lock)
            {
                int nextId = dashboard.EquipmentExpenses.Any() ? dashboard.EquipmentExpenses.Max(e => e.Id) + 1 : 1;
                dashboard.EquipmentExpenses.Insert(0, new TaseronEquipmentExpenseViewModel
                {
                    Id = nextId,
                    ItemName = itemName,
                    SupplierName = supplierName,
                    Amount = amount,
                    Date = DateTime.UtcNow,
                    Category = category ?? "İskele & Makine Kiralama",
                    Status = "Açık Hesap Borcu"
                });
                RecalculateTotals(dashboard);
                SaveToDisk(dashboard);
            }
            return true;
        }

        public async Task<bool> ToggleBranchAsync(string branchName)
        {
            var dashboard = await GetTaseronDashboardAsync();
            lock (_lock)
            {
                if (dashboard.Branches.Contains(branchName))
                {
                    dashboard.Branches.Remove(branchName);
                }
                else
                {
                    dashboard.Branches.Add(branchName);
                }
                SaveToDisk(dashboard);
            }
            return true;
        }

        public string GenerateContractWhatsAppSummary(TaseronContractViewModel contract, string taseronName, string taseronPhone)
        {
            var msg = $"Sayın {contract.SiteManagerContact} ({contract.ContractorCompany}),\n" +
                      $"GMK360 Taşeron Resmî Hakediş Mutabakat Talebimizdir:\n\n" +
                      $"🏗️ Şantiye: {contract.ProjectName}\n" +
                      $"🔨 İmalat Kapsamı: {contract.WorkScope}\n" +
                      $"📊 Sözleşme Türü: {contract.ContractType} ({contract.TotalQuantityMetraj:N0} m² x {contract.UnitPrice:N0} TL)\n" +
                      $"💰 Bu Dönem Hakediş Talebi: {contract.CurrentPeriodHakedisClaim:N0} TL\n\n" +
                      $"🛡️ MÜTEAHHİT KESİNTİ KALKANI:\n" +
                      $"• Teminat Kesintisi (%{contract.RetentionGuaranteeRate}): -{contract.RetentionDeductionAmount:N0} TL (Geçici kabulde iade)\n" +
                      $"• Şantiye Elektrik/Su: -{contract.SiteElectricWaterDeduction:N0} TL\n" +
                      $"• Stopaj/Gider Pusulası: 0 TL (Yasal olarak müteahhide aittir, hakedişten kesilemez)\n" +
                      $"💵 NET ÖDENECEK HAKEDİŞ: {contract.NetPayableHakedis:N0} TL\n\n" +
                      $"👨‍🔧 Taşeron Firma: {taseronName} ({taseronPhone})\n" +
                      $"Bu mesajı inceleyip onaylamanızı rica ederiz.";

            var cleanPhone = new string(contract.SiteManagerContact.Where(char.IsDigit).ToArray());
            if (cleanPhone.StartsWith("0")) cleanPhone = "90" + cleanPhone.Substring(1);
            else if (!cleanPhone.StartsWith("90") && cleanPhone.Length == 10) cleanPhone = "90" + cleanPhone;

            return $"https://api.whatsapp.com/send?phone={cleanPhone}&text={Uri.EscapeDataString(msg)}";
        }

        private void RecalculateTotals(TaseronDashboardViewModel vm)
        {
            vm.TotalContractVolume = vm.Contracts.Sum(c => c.TotalContractAmount);
            vm.TotalCollectedHakedis = vm.Contracts.Sum(c => c.CumulativeHakedisPaid);
            vm.CurrentPeriodHakedis = vm.Contracts.Sum(c => c.CurrentPeriodHakedisClaim);

            vm.TotalWorkerWagesThisPeriod = vm.Workers.Sum(w => w.TotalWagesEarned);
            vm.TotalAdvancesGivenThisPeriod = vm.Advances.Sum(a => a.Amount);
            vm.TotalEquipmentExpenseThisPeriod = vm.EquipmentExpenses.Sum(e => e.Amount);
            vm.TotalSiteOverheadThisPeriod = 22000m; // Şantiye servis & öğle yemeği

            vm.NetProfitThisPeriod = vm.CurrentPeriodHakedis - (vm.TotalWorkerWagesThisPeriod + vm.TotalEquipmentExpenseThisPeriod + vm.TotalSiteOverheadThisPeriod);

            foreach (var c in vm.Contracts)
            {
                c.WhatsAppHakedisSummaryUrl = GenerateContractWhatsAppSummary(c, vm.CompanyTitle, vm.PhoneNumber);
            }
        }

        private void SaveToDisk(TaseronDashboardViewModel vm)
        {
            try
            {
                var path = GetStorageFilePath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(vm, options);
                File.WriteAllText(path, json);
            }
            catch { }
        }

        private TaseronDashboardViewModel BuildSampleTaseron()
        {
            var vm = new TaseronDashboardViewModel
            {
                TaseronId = 1,
                CompanyTitle = "Kaya Kaba İnşaat & Kalıp-Demir Taşeronluğu",
                ManagerName = "Hasan Kaya (Kaya Usta)",
                PhoneNumber = "0533 888 77 66",
                CityName = "İstanbul",
                DistrictName = "Kadıköy / Ataşehir",
                Bio = "15 yıldır İstanbul genelinde konut, villa ve iş merkezi projelerinde kalıp, demir donatı, beton dökümü, duvar ve kaba yapı taşeronluğu.",
                IsCityWideSubscriptionActive = true,
                MonthlySubscriptionFee = 250m,
                IsVerifiedSubcontractor = true,
                ExperienceYears = 15,

                Branches = new List<string>
                {
                    "Kalıp & Ahşap İşçiliği",
                    "Nervürlü Demir Bağlama & Donatı",
                    "Tuğla, Bims & Gazbeton Duvar",
                    "Şap & Kaba Sıva",
                    "Bahçe Düzenleme, Peyzaj & Bahçıvanlık",
                    "Kırım & Moloz Hafriyat"
                },

                Contracts = new List<TaseronContractViewModel>
                {
                    new TaseronContractViewModel
                    {
                        Id = 1,
                        ProjectName = "Beyaz Konak Rezidans (10 Katlı Konut)",
                        ContractorCompany = "Dengehan İnşaat A.Ş.",
                        SiteManagerContact = "Şantiye Şefi Murat Bey (0532 111 22 33)",
                        WorkScope = "10 Katlı Blok Kaba İnşaat (Kalıp + Demir + Duvar)",
                        ContractType = "Metraj / Birim Fiyatlı",
                        UnitPrice = 380m,
                        TotalQuantityMetraj = 5000,
                        TotalContractAmount = 1900000m,
                        CumulativeHakedisPaid = 850000m,
                        CurrentPeriodHakedisClaim = 350000m,
                        RetentionGuaranteeRate = 5m,
                        SiteElectricWaterDeduction = 2500m,
                        Status = "Devam Ediyor",
                        StartDate = DateTime.UtcNow.AddMonths(-3),
                        EstimatedCompletionDate = DateTime.UtcNow.AddMonths(4)
                    },
                    new TaseronContractViewModel
                    {
                        Id = 2,
                        ProjectName = "Ataşehir Park Evleri 4 Blok İstinat Duvarı & Çevre",
                        ContractorCompany = "Örnek Yapı Sanayi Ltd.",
                        SiteManagerContact = "Proje Müdürü Selim Bey (0533 444 88 99)",
                        WorkScope = "Perde Beton İstinat Duvarı & Peyzaj Kaba Taş Duvarı",
                        ContractType = "Götürü İş Bedeli",
                        UnitPrice = 450000m,
                        TotalQuantityMetraj = 1,
                        TotalContractAmount = 450000m,
                        CumulativeHakedisPaid = 200000m,
                        CurrentPeriodHakedisClaim = 120000m,
                        RetentionGuaranteeRate = 5m,
                        SiteElectricWaterDeduction = 1000m,
                        Status = "Devam Ediyor",
                        StartDate = DateTime.UtcNow.AddMonths(-1),
                        EstimatedCompletionDate = DateTime.UtcNow.AddMonths(2)
                    }
                },

                Workers = new List<TaseronWorkerViewModel>
                {
                    new TaseronWorkerViewModel
                    {
                        Id = 1,
                        FullName = "Recep Demir (Demirci Başı)",
                        PhoneNumber = "0535 222 33 44",
                        TradeRole = "Demir Donatı Ustası",
                        DailyWage = 1600m,
                        DaysWorkedThisMonth = 24,
                        TotalAdvancesTaken = 9000m,
                        IsRegisteredFutureCraftsman = true
                    },
                    new TaseronWorkerViewModel
                    {
                        Id = 2,
                        FullName = "Mehmet Yıldız (Kalıp Ustası)",
                        PhoneNumber = "0536 333 44 55",
                        TradeRole = "Kalıp & Ahşap Ustası",
                        DailyWage = 1600m,
                        DaysWorkedThisMonth = 24,
                        TotalAdvancesTaken = 8500m,
                        IsRegisteredFutureCraftsman = true
                    },
                    new TaseronWorkerViewModel
                    {
                        Id = 3,
                        FullName = "Bilal Çelik",
                        PhoneNumber = "0537 444 55 66",
                        TradeRole = "Demir Bağlama Ustası",
                        DailyWage = 1400m,
                        DaysWorkedThisMonth = 23,
                        TotalAdvancesTaken = 6000m,
                        IsRegisteredFutureCraftsman = true
                    },
                    new TaseronWorkerViewModel
                    {
                        Id = 4,
                        FullName = "Salih Kaynak",
                        PhoneNumber = "0538 555 66 77",
                        TradeRole = "Sertifikalı Kaynakçı",
                        DailyWage = 1500m,
                        DaysWorkedThisMonth = 22,
                        TotalAdvancesTaken = 7000m,
                        IsRegisteredFutureCraftsman = true
                    },
                    new TaseronWorkerViewModel
                    {
                        Id = 5,
                        FullName = "İbrahim Taşçı",
                        PhoneNumber = "0539 666 77 88",
                        TradeRole = "Tuğla & Duvar Ustası",
                        DailyWage = 1400m,
                        DaysWorkedThisMonth = 24,
                        TotalAdvancesTaken = 6500m,
                        IsRegisteredFutureCraftsman = true
                    },
                    new TaseronWorkerViewModel
                    {
                        Id = 6,
                        FullName = "Ferhat Çetin",
                        PhoneNumber = "0540 777 88 99",
                        TradeRole = "İnşaat Kalfası / Vinç Operatörü",
                        DailyWage = 1300m,
                        DaysWorkedThisMonth = 24,
                        TotalAdvancesTaken = 5000m,
                        IsRegisteredFutureCraftsman = true
                    }
                },

                RecentPuantajEntries = new List<TaseronPuantajEntryViewModel>
                {
                    new TaseronPuantajEntryViewModel
                    {
                        Id = 1,
                        Date = DateTime.UtcNow,
                        WorkerId = 1,
                        WorkerName = "Recep Demir (Demirci Başı)",
                        TradeRole = "Demir Donatı Ustası",
                        WorkDayFactor = 1.0,
                        OvertimeHours = 2,
                        SiteLocation = "Beyaz Konak Rezidans",
                        Notes = "3. Kat tabliye donatısı tamamlandı, fazla mesai ile akşama bağlandı."
                    },
                    new TaseronPuantajEntryViewModel
                    {
                        Id = 2,
                        Date = DateTime.UtcNow,
                        WorkerId = 2,
                        WorkerName = "Mehmet Yıldız (Kalıp Ustası)",
                        TradeRole = "Kalıp & Ahşap Ustası",
                        WorkDayFactor = 1.0,
                        OvertimeHours = 0,
                        SiteLocation = "Beyaz Konak Rezidans",
                        Notes = "Kolon kalıpları şakülüne getirildi ve kilitlendi."
                    },
                    new TaseronPuantajEntryViewModel
                    {
                        Id = 3,
                        Date = DateTime.UtcNow.AddDays(-1),
                        WorkerId = 3,
                        WorkerName = "Bilal Çelik",
                        TradeRole = "Demir Bağlama Ustası",
                        WorkDayFactor = 1.0,
                        OvertimeHours = 0,
                        SiteLocation = "Beyaz Konak Rezidans",
                        Notes = "Kiriş donatıları montajı."
                    }
                },

                Advances = new List<TaseronAdvanceViewModel>
                {
                    new TaseronAdvanceViewModel
                    {
                        Id = 1,
                        Date = DateTime.UtcNow.AddDays(-2),
                        WorkerId = 1,
                        WorkerName = "Recep Demir (Demirci Başı)",
                        Amount = 4000m,
                        PaymentType = "Elden Nakit Cuma Harçlığı",
                        Notes = "Haftalık harçlık teslim edildi."
                    },
                    new TaseronAdvanceViewModel
                    {
                        Id = 2,
                        Date = DateTime.UtcNow.AddDays(-2),
                        WorkerId = 2,
                        WorkerName = "Mehmet Yıldız (Kalıp Ustası)",
                        Amount = 3500m,
                        PaymentType = "Elden Nakit Cuma Harçlığı",
                        Notes = "Haftalık avans ödendi."
                    }
                },

                EquipmentExpenses = new List<TaseronEquipmentExpenseViewModel>
                {
                    new TaseronEquipmentExpenseViewModel
                    {
                        Id = 1,
                        ItemName = "200 Adet Teleskopik Kalıp Direği & Çelik İskele (Aylık)",
                        SupplierName = "Mega İskele & Kalıp Kiralama",
                        Amount = 28000m,
                        Date = DateTime.UtcNow.AddDays(-5),
                        Category = "İskele & Makine Kiralama",
                        Status = "Açık Hesap Borcu"
                    },
                    new TaseronEquipmentExpenseViewModel
                    {
                        Id = 2,
                        ItemName = "15 Top Tavlı Bağ Teli + 200 kg 5'lik Çivi + Paspayı Takozu",
                        SupplierName = "Özkan Yapı & Nalburiye",
                        Amount = 8500m,
                        Date = DateTime.UtcNow.AddDays(-8),
                        Category = "Sarf Malzeme",
                        Status = "Ödendi"
                    }
                }
            };

            RecalculateTotals(vm);
            return vm;
        }
    }
}
