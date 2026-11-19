using System;

namespace GMK360.Web.Helpers
{
    public sealed class FacilityPricingResult
    {
        public string TierName { get; set; } = string.Empty;
        public decimal AnnualPrice { get; set; }
        public int BlockCount { get; set; }
        public int UnitCount { get; set; }
        public int TrialDays { get; set; } = 14;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Apartman, kücük villa sitesi ve çok bloklu siteler için blok + daire sayısına göre adil yıllık paket hesabı.
    /// </summary>
    public static class FacilityPricingHelper
    {
        public static FacilityPricingResult Calculate(bool isSite, int blockCount, int totalUnits)
        {
            int normalizedBlocks = Math.Max(1, blockCount);
            int normalizedUnits = Math.Max(0, totalUnits);

            // Tekil yapı (1 blok): 30 daire altı 5.000 TL, 30 ve üstü 7.500 TL
            if (!isSite && normalizedBlocks <= 1)
            {
                if (normalizedUnits < 30)
                {
                    return new FacilityPricingResult
                    {
                        TierName = "Apartman Başlangıç",
                        AnnualPrice = 5000m,
                        BlockCount = normalizedBlocks,
                        UnitCount = normalizedUnits,
                        Description = "Tekil bina (1 blok, 30 daire altı)"
                    };
                }

                return new FacilityPricingResult
                {
                    TierName = "Geniş Apartman",
                    AnnualPrice = 7500m,
                    BlockCount = normalizedBlocks,
                    UnitCount = normalizedUnits,
                    Description = "Tekil bina (1 blok, 30 daire ve üstü)"
                };
            }

            // Çoklu bloklu site / villa grubu:
            // Küçük villa sitesi koruması: 2+ blok olup toplam < 30 daire/birim ise 7.500 TL
            if (normalizedUnits < 30)
            {
                return new FacilityPricingResult
                {
                    TierName = "Küçük Site / Villa Grubu",
                    AnnualPrice = 7500m,
                    BlockCount = normalizedBlocks,
                    UnitCount = normalizedUnits,
                    Description = $"{normalizedBlocks} bloklu, 30 birim altı (villa / butik site)"
                };
            }

            // 3 ve üstü blok VE 100+ birim => Büyük Çok Bloklu Site 15.000 TL
            if (normalizedBlocks >= 3 && normalizedUnits >= 100)
            {
                return new FacilityPricingResult
                {
                    TierName = "Büyük Çok Bloklu Site",
                    AnnualPrice = 15000m,
                    BlockCount = normalizedBlocks,
                    UnitCount = normalizedUnits,
                    Description = $"{normalizedBlocks} bloklu ve {normalizedUnits} birimli büyük yerleşke"
                };
            }

            // Orta ölçekli site (2 blok 30+ birim veya 3+ blok 30-99 birim) => 10.000 TL
            return new FacilityPricingResult
            {
                TierName = "Orta Ölçekli Site",
                AnnualPrice = 10000m,
                BlockCount = normalizedBlocks,
                UnitCount = normalizedUnits,
                Description = $"{normalizedBlocks} bloklu, {normalizedUnits} bağımsız bölümlü site"
            };
        }
    }
}
