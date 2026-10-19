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
    public interface IRealtorWorkspaceService
    {
        Task<RealtorDashboardViewModel> GetDashboardAsync();
        Task<bool> AddNegotiationNoteAsync(string propertyTitle, string buyerName, string buyerPhone, decimal buyerOfferedPrice, string whatIToldBuyer, decimal sellerBottomPrice, string whatIToldSeller, string secretNotes);
        Task<bool> AddShowingAppointmentAsync(DateTime appointmentDate, string clientName, string clientPhone, string propertyTitle, string location, string feedback, bool interestedInRenovation);
        Task<bool> AddListingAsync(string title, string listingType, string location, decimal price, string roomCount, int areaM2, string assignedConsultant);
        Task<bool> AssignConsultantToListingAsync(int listingId, string consultantName);
        Task<bool> AddReferredLandlordAsync(string landlordName, string propertyAddress);
        string GenerateFestivalGreetingMessage(string clientName, string brokerName, string agencyName);
        string GenerateTadilatAdvisoryText(string propertyTitle, decimal m2);
    }

    public class RealtorWorkspaceService : IRealtorWorkspaceService
    {
        private readonly IWebHostEnvironment _env;
        private static RealtorDashboardViewModel? _cachedData;
        private static readonly object _lock = new();

        public RealtorWorkspaceService(IWebHostEnvironment env)
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
            return Path.Combine(dataDir, "realtor_workspace.json");
        }

        public async Task<RealtorDashboardViewModel> GetDashboardAsync()
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
                        _cachedData = JsonSerializer.Deserialize<RealtorDashboardViewModel>(json);
                        if (_cachedData != null) return _cachedData;
                    }
                    catch
                    {
                        // JSON deserialization fallback
                    }
                }

                _cachedData = CreateDefaultData();
                SaveToFile(_cachedData);
                return _cachedData;
            }
        }

        public async Task<bool> AddNegotiationNoteAsync(string propertyTitle, string buyerName, string buyerPhone, decimal buyerOfferedPrice, string whatIToldBuyer, decimal sellerBottomPrice, string whatIToldSeller, string secretNotes)
        {
            var data = await GetDashboardAsync();
            lock (_lock)
            {
                int nextId = data.NegotiationNotes.Any() ? data.NegotiationNotes.Max(n => n.Id) + 1 : 1;
                data.NegotiationNotes.Insert(0, new RealtorNegotiationSecretNote
                {
                    Id = nextId,
                    PropertyTitle = propertyTitle,
                    BuyerName = buyerName,
                    BuyerPhone = buyerPhone,
                    BuyerOfferedPrice = buyerOfferedPrice,
                    WhatIToldBuyer = whatIToldBuyer,
                    SellerBottomPrice = sellerBottomPrice,
                    WhatIToldSeller = whatIToldSeller,
                    SecretNotes = secretNotes,
                    UpdatedAt = DateTime.UtcNow
                });
                SaveToFile(data);
                return true;
            }
        }

        public async Task<bool> AddShowingAppointmentAsync(DateTime appointmentDate, string clientName, string clientPhone, string propertyTitle, string location, string feedback, bool interestedInRenovation)
        {
            var data = await GetDashboardAsync();
            lock (_lock)
            {
                int nextId = data.ShowingAppointments.Any() ? data.ShowingAppointments.Max(s => s.Id) + 1 : 1;
                data.ShowingAppointments.Insert(0, new RealtorShowingAppointment
                {
                    Id = nextId,
                    AppointmentDate = appointmentDate,
                    ClientName = clientName,
                    ClientPhone = clientPhone,
                    PropertyTitle = propertyTitle,
                    Location = location,
                    ShowingReportStatus = "İmzalandı (Dijital)",
                    ClientFeedback = feedback,
                    IsInterestedInRenovation = interestedInRenovation
                });
                SaveToFile(data);
                return true;
            }
        }

        public async Task<bool> AddListingAsync(string title, string listingType, string location, decimal price, string roomCount, int areaM2, string assignedConsultant)
        {
            var data = await GetDashboardAsync();
            lock (_lock)
            {
                int nextId = data.Listings.Any() ? data.Listings.Max(l => l.Id) + 1 : 1;
                data.Listings.Insert(0, new RealtorListingSummaryItem
                {
                    Id = nextId,
                    Title = title,
                    ListingType = listingType,
                    Location = location,
                    Price = price,
                    RoomCount = roomCount,
                    AreaM2 = areaM2,
                    ListedDate = DateTime.UtcNow,
                    ViewCount = 1,
                    ShowingCount = 0,
                    Status = "Yayında (Sınırsız Kotasız)",
                    AssignedConsultantName = string.IsNullOrEmpty(assignedConsultant) ? "Ofis Portföyü" : assignedConsultant,
                    ImageUrl = "/images/default-property.jpg"
                });
                SaveToFile(data);
                return true;
            }
        }

        public async Task<bool> AssignConsultantToListingAsync(int listingId, string consultantName)
        {
            var data = await GetDashboardAsync();
            lock (_lock)
            {
                var listing = data.Listings.FirstOrDefault(l => l.Id == listingId);
                if (listing != null)
                {
                    listing.AssignedConsultantName = consultantName;
                    SaveToFile(data);
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> AddReferredLandlordAsync(string landlordName, string propertyAddress)
        {
            var data = await GetDashboardAsync();
            lock (_lock)
            {
                data.PassiveIncome.ReferredLandlordsCount += 1;
                SaveToFile(data);
                return true;
            }
        }

        public string GenerateFestivalGreetingMessage(string clientName, string brokerName, string agencyName)
        {
            return $"Sayın {clientName}, sevdiklerinizle birlikte sağlıklı, huzurlu ve bereketli bayramlar dileriz. Yuvanızdan neşe ve huzur eksik olmasın. En içten dileklerimizle, {brokerName} ({agencyName} / GMK360 Ekosistemi).";
        }

        public string GenerateTadilatAdvisoryText(string propertyTitle, decimal m2)
        {
            decimal ecCost = m2 * 1150m;
            decimal stCost = m2 * 2300m;
            decimal lxCost = m2 * 4100m;

            return $"*{propertyTitle} - GMK360 Anahtar Teslim Tadilat Danışmanlık Özeti ({m2} m²)*\n\n" +
                   $"1. *Ekonomik Paket:* {ecCost:N0} ₺ (Boya badana, armatür değişimi, hafif kozmetik yenileme)\n" +
                   $"2. *Standart Paket:* {stCost:N0} ₺ (Mutfak dolapları, banyo seramikleri, su tesisatı, parke, boya)\n" +
                   $"3. *Lüks Konsept:* {lxCost:N0} ₺ (Akıllı ev altyapısı, ada tezgah, ithal seramik, gizli aydınlatma, anahtar teslim)\n\n" +
                   $"_GMK360 Akredite Usta & Nalbur Ağı Güvencesiyle Resmi Teklif Alınabilir._";
        }

        private void SaveToFile(RealtorDashboardViewModel data)
        {
            try
            {
                var path = GetStorageFilePath();
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch
            {
                // File writing fallback
            }
        }

        private RealtorDashboardViewModel CreateDefaultData()
        {
            var vm = new RealtorDashboardViewModel
            {
                AgencyName = "Moda Portföy & Mega Gayrimenkul",
                BrokerName = "Serdar Korkmaz (Broker/Owner)",
                IsMegaBrokerOffice = true
            };

            // 1. Sınırsız İlanlar
            vm.Listings.AddRange(new[]
            {
                new RealtorListingSummaryItem
                {
                    Id = 201,
                    Title = "Moda Caddesi Geniş Balkonlu 3+1 Kombili Masrafsız Daire",
                    ListingType = "Satılık Konut",
                    Location = "Kadıköy / Caferağa (Moda)",
                    Price = 7250000,
                    RoomCount = "3+1",
                    AreaM2 = 135,
                    ListedDate = DateTime.UtcNow.AddDays(-12),
                    ViewCount = 1420,
                    ShowingCount = 9,
                    Status = "Yayında (Sınırsız)",
                    AssignedConsultantName = "Ahmet Yılmaz (Moda Uzmanı)"
                },
                new RealtorListingSummaryItem
                {
                    Id = 202,
                    Title = "Bağdat Caddesi'ne 2. Paralel Asansörlü Otoparklı 2+1",
                    ListingType = "Satılık Konut",
                    Location = "Kadıköy / Caddebostan",
                    Price = 9800000,
                    RoomCount = "2+1",
                    AreaM2 = 98,
                    ListedDate = DateTime.UtcNow.AddDays(-5),
                    ViewCount = 890,
                    ShowingCount = 6,
                    Status = "Görüşme Sürüyor",
                    AssignedConsultantName = "Zeynep Kaya (Cadde Portföy)"
                },
                new RealtorListingSummaryItem
                {
                    Id = 203,
                    Title = "Fenerbahçe Marina Manzaralı Eşyalı Dubleks",
                    ListingType = "Kiralık Daire",
                    Location = "Kadıköy / Fenerbahçe",
                    Price = 45000,
                    RoomCount = "4+2",
                    AreaM2 = 220,
                    ListedDate = DateTime.UtcNow.AddDays(-3),
                    ViewCount = 640,
                    ShowingCount = 4,
                    Status = "Yayında (Sınırsız)",
                    AssignedConsultantName = "Burak Demir"
                }
            });

            // 2. "Alıcıya Ne Dedim? / Satıcıya Ne Dedim?" Gizli Pazarlık Defteri
            vm.NegotiationNotes.AddRange(new[]
            {
                new RealtorNegotiationSecretNote
                {
                    Id = 1,
                    PropertyTitle = "Moda Caddesi 3+1 Daire (İlan #201)",
                    BuyerName = "Dr. Kemal Arslan",
                    BuyerPhone = "0532 444 88 12",
                    BuyerOfferedPrice = 6900000,
                    WhatIToldBuyer = "Ev sahibinin 7.150.000 TL taban rakamı olduğunu, banyo ve mutfak masrafı için 150 bin TL tadilat indirimi koparmaya çalışacağımı belirttim.",
                    SellerBottomPrice = 6950000,
                    WhatIToldSeller = "Alıcının nakit 6.900.000 TL teklif ettiğini, piyasanın durgun olduğunu ve 7.000.000 TL'ye el sıkıştırabileceğimizi söyledim.",
                    SecretNotes = "Alıcının eşi mutfağı kesinlikle yenilemek istiyor. GMK360 Usta tadilat maliyet tablosunu çıkarıp aradaki farkı kapatacağız.",
                    UpdatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new RealtorNegotiationSecretNote
                {
                    Id = 2,
                    PropertyTitle = "Caddebostan 2+1 (İlan #202)",
                    BuyerName = "Merve & Tolga Çifti",
                    BuyerPhone = "0555 777 23 45",
                    BuyerOfferedPrice = 9400000,
                    WhatIToldBuyer = "Mal sahibinin acele etmediğini ama 9.600.000 TL nakit teklifte tapu masrafını bölüşme sözü alabileceğimi ilettim.",
                    SellerBottomPrice = 9500000,
                    WhatIToldSeller = "Müşterinin kredi onayı hazır, 9.500.000 TL'ye hemen tapu yapmaya hazır olduklarını bildirdim.",
                    SecretNotes = "Satıcı İzmir'e taşınacağı için haftaya kadar imzayı atar. Pazarlık payı dar.",
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            });

            // 3. Yer Gösterme Takvimi
            vm.ShowingAppointments.AddRange(new[]
            {
                new RealtorShowingAppointment
                {
                    Id = 1,
                    AppointmentDate = DateTime.UtcNow.AddHours(4),
                    ClientName = "Hakan Gündüz",
                    ClientPhone = "0542 333 11 99",
                    PropertyTitle = "Moda Caddesi 3+1",
                    Location = "Caferağa Mah. No: 14 D: 3",
                    ShowingReportStatus = "İmzalandı (SMS Onaylı)",
                    ClientFeedback = "Konum mükemmel, banyoda tadilat istiyor.",
                    IsInterestedInRenovation = true
                },
                new RealtorShowingAppointment
                {
                    Id = 2,
                    AppointmentDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                    ClientName = "Selin Varol",
                    ClientPhone = "0533 111 44 22",
                    PropertyTitle = "Caddebostan 2+1",
                    Location = "Caddebostan Mah. Plaj Yolu",
                    ShowingReportStatus = "Randevu Teyit Edildi",
                    ClientFeedback = "Otopark durumunu özellikle sordu.",
                    IsInterestedInRenovation = false
                }
            });

            // 4. Kazan-Kazan Pasif Gelir Sayacı
            vm.PassiveIncome = new RealtorPassiveIncomeTracker
            {
                ReferredLandlordsCount = 7, // 5'i geçtiği için aidat 0 TL!
                TargetForFreePlan = 5,
                SymbolAnnualFee = 3500m
            };

            // 5. Tadilat & Yatırım Danışmanlığı Şablonu
            vm.RenovationQuotes.Add(new RealtorRenovationAdvisoryQuote
            {
                PropertyTitle = "Moda 3+1 Daire (135 m²)",
                EconomicPackageCost = 155000,
                StandardPackageCost = 310000,
                LuxuryPackageCost = 550000,
                ScopeSummary = "Mutfak lake dolaplar, granit tezgah, banyo Kalebodur seramik, Vitra gömme rezervuar, tesisat yenileme, Jotun boya.",
                NetworkSource = "GMK360 Kadıköy Bölgesi Akredite Usta & Nalbur Birliği"
            });

            // 6. Mega Broker / Remax Danışman Ekibi
            vm.BrokerTeam.AddRange(new[]
            {
                new RealtorBrokerTeamMember
                {
                    Id = 1,
                    FullName = "Ahmet Yılmaz",
                    Title = "Kıdemli Gayrimenkul Danışmanı",
                    SpecializedTerritory = "Kadıköy / Moda & Caferağa",
                    ActiveListingsCount = 18,
                    MonthlyDealsClosed = 3,
                    CommissionSharePercent = 50,
                    Phone = "0532 999 11 22"
                },
                new RealtorBrokerTeamMember
                {
                    Id = 2,
                    FullName = "Zeynep Kaya",
                    Title = "Lüks Konut Uzmanı",
                    SpecializedTerritory = "Kadıköy / Caddebostan & Suadiye",
                    ActiveListingsCount = 14,
                    MonthlyDealsClosed = 2,
                    CommissionSharePercent = 55,
                    Phone = "0533 888 33 44"
                },
                new RealtorBrokerTeamMember
                {
                    Id = 3,
                    FullName = "Burak Demir",
                    Title = "Ticari & Arsa Danışmanı",
                    SpecializedTerritory = "Kadıköy / Koşuyolu & Acıbadem",
                    ActiveListingsCount = 9,
                    MonthlyDealsClosed = 1,
                    CommissionSharePercent = 45,
                    Phone = "0544 777 55 66"
                }
            });

            // 7. Müteahhit Proje Tanıtım Vitrini (Satış/Topraktan kelimesi YOK!)
            vm.ProjectPartnerships.AddRange(new[]
            {
                new RealtorProjectPartnershipItem
                {
                    ProjectId = 501,
                    ProjectName = "Mavi Çamlıca Yaşam Konakları",
                    ConstructionCompanyName = "Korkmaz İnşaat Taahhüt A.Ş.",
                    Territory = "Üsküdar / Çamlıca Tepesi",
                    EstimatedDelivery = "2026 4. Çeyrek",
                    PartnershipModel = "Tüm GMK360 Emlakçılarına Açık Bilgilendirme Yetkisi",
                    CommissionRate = 2.5m,
                    PreDemandLeadsCount = 18,
                    ProjectHighlight = "Mimari Lansman Öncesi Ön Talep Kataloğu — Kat Planları ve Ön Görüşme Kaydı"
                },
                new RealtorProjectPartnershipItem
                {
                    ProjectId = 502,
                    ProjectName = "Moda Sahil Prestij Rezidans",
                    ConstructionCompanyName = "Ege Yapı Mimarlık Grubu",
                    Territory = "Kadıköy / Moda Sahil",
                    EstimatedDelivery = "2027 2. Çeyrek",
                    PartnershipModel = "VIP Tek Yetkili Acente Temsilciliği",
                    CommissionRate = 3.0m,
                    PreDemandLeadsCount = 27,
                    ProjectHighlight = "Bölgesel Ön Görüşme & Lansman Bilgilendirme Listesi"
                }
            });

            return vm;
        }
    }
}
