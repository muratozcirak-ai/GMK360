using System;

namespace GMK360.Core.DTOs.Construction
{
    public class ProjectRentPolicyDto
    {
        // 🏠 Daire Politikası
        public decimal FlatRelocationFee { get; set; } = 40000m;
        public decimal FlatMonthlyRent { get; set; } = 30000m;
        public DateTime? FlatStartDate { get; set; }
        public DateTime? FlatEndDate { get; set; }
        public int FlatPaymentDay { get; set; } = 5;
        public int FlatGraceDays { get; set; } = 3;

        // 🏢 Dükkan Politikası
        public decimal ShopRelocationFee { get; set; } = 50000m;
        public decimal ShopMonthlyRent { get; set; } = 45000m;
        public DateTime? ShopStartDate { get; set; }
        public DateTime? ShopEndDate { get; set; }
        public int ShopPaymentDay { get; set; } = 5;
        public int ShopGraceDays { get; set; } = 3;

        // ====================================================================
        // 1. SAYFA (KÜNYE): 4 ALTIN EVRAK & İLK KABUL FİLTRESİ
        // ====================================================================
        public string BuildingOrderType { get; set; } = "Tek Yandan Bitişik Nizam"; // Ayrık Nizam, Tek Yandan Bitişik Nizam, İki Yandan Bitişik Nizam
        public int OwnerPreConsensusRate { get; set; } = 75; // Malik Ön Uzlaşı Oranı (%)
        public string? ImarCapiDocumentUrl { get; set; }
        public string? KararDefteriDocumentUrl { get; set; }

        // ====================================================================
        // 2. SAYFA (HIZLI ÖN ETÜD): TERAZİ & DÜKKAN / KİRA SİMÜLASYONU
        // ====================================================================
        public int UnitsPerFloor { get; set; } = 2;
        public int BasementFloors { get; set; } = 2;
        public decimal CoreLossSqm { get; set; } = 35m;
        public decimal OwnerSharePercent { get; set; } = 50m;
        public decimal FirmUnitCostPerSqm { get; set; } = 25000m;
        public int PlannedNewShops { get; set; } = 2;
        public int OwnerGivenShops { get; set; } = 1;
        public decimal EstimatedShopSalePrice { get; set; } = 15000000m;
        public bool IncludeRentInFeasibility { get; set; } = true;
        public int RentDurationMonths { get; set; } = 18;

        // ====================================================================
        // 3. SAYFA (DETAYLI İNCELEME): 5 BOYUTLU RÖNTGEN & GO/NO-GO KARAR MASASI
        // ====================================================================
        // 1) İmar & Kapasite
        public decimal OnayliEmsal { get; set; } = 2.00m;
        public decimal TabanOturumuTaks { get; set; } = 0.35m;
        public int TargetFlatGrossSqm { get; set; } = 100;

        // 2) Hukuki & Malik Taraması
        public string TitleDeedEncumbranceStatus { get; set; } = "Temiz"; // Temiz, BankaIpotegiVar, KamuHacziSerhVar
        public string InheritanceStatus { get; set; } = "SorunYok"; // SorunYok, VerasetBekliyor, MirascilarKavgaliDavali
        public string HostileOwnerStatus { get; set; } = "TamUzlasi"; // TamUzlasi, SaltCogunlukHazir, ArizaMalikVar
        public string? LegalCheckNotes { get; set; }

        // 3) Saha, Komşu Bina & Bitişik Nizam / Yıkım & İksa Keşfi
        public string AdjacentBuildingRisk { get; set; } = "TekYandanBitisik"; // AyrikRiskYok, TekYandanBitisik, IkiYandanBitisikEskiBina
        public int AdjacentBuildingAge { get; set; } = 40;
        public string ShoringRequirement { get; set; } = "KuyuTemel"; // Gerekmez, KuyuTemel, MiniKazikAnkrajliIksa
        public decimal EstimatedShoringCost { get; set; } = 1250000m;
        public string StreetLogisticsStatus { get; set; } = "GenisSokakUygun"; // GenisSokakUygun, DarSokakOzelEkipman, CokDarTrafikIzniZor
        public string DemolitionExecutionMode { get; set; } = "DisaridanYikimFirmasi"; // DisaridanYikimFirmasi, KendiBunyemizde
        public bool RequireDemolitionShield { get; set; } = true;
        public decimal DemolitionExtraPolicyCost { get; set; } = 150000m;

        // 4) Zemin, İstinat ve Kazık Risk Masası
        public string LandSlopeStatus { get; set; } = "DuzArsa"; // DuzArsa, MeyilliSevliArsa
        public decimal RetainingWallHeightMeters { get; set; } = 0m;
        public decimal EstimatedRetainingWallCost { get; set; } = 0m;
        public string SoilBearingStatus { get; set; } = "SaglamKaya"; // SaglamKaya, YumusakBalcikSivilasma
        public int EstimatedPileCount { get; set; } = 0;
        public decimal EstimatedPilingCost { get; set; } = 0m;
        public string GroundwaterStatus { get; set; } = "SuYok"; // SuYok, YuksekSuSeviyesi
        public decimal EstimatedGroundwaterCost { get; set; } = 0m;
        public string ExtraGroundCostPayer { get; set; } = "ArsaSahipleriIlePaylasilir"; // ArsaSahipleriIlePaylasilir, TamamiMaliklereAit, MuteahhitUstlenir

        // 5) Belediye Yıkım İzni & Karot Analizi
        public string CoreTestStatus { get; set; } = "RiskliYapi6306Uygun"; // RiskliYapi6306Uygun, HenuzKarotAlinmadi, BinaSaglamYikimRiskli
        public string RegionalHarmonyStatus { get; set; } = "CevreIleUyumluKatArtisiVar"; // CevreIleUyumluKatArtisiVar, MevcutKatKorunuyor, ImarKaybiRiskiVar

        // Nihai GO / NO-GO Kararı ve Teklif & Şartname Kalkanı
        public string GoNoGoDecision { get; set; } = "ConditionalGo"; // Go, ConditionalGo, NoGo
        public string? ContractShieldClauses { get; set; }
        public decimal BankGuaranteeRate { get; set; } = 10m;
    }
}
