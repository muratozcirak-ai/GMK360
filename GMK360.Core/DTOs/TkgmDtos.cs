using System.Collections.Generic;

namespace GMK360.Core.DTOs
{
    public class TkgmAdministrativeUnitDto
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Name => Text;
    }

    public class TkgmParcelResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public string? IlAd { get; set; }
        public int? IlId { get; set; }
        public string? IlceAd { get; set; }
        public int? IlceId { get; set; }
        public string? MahalleAd { get; set; }
        public long? MahalleId { get; set; }

        public string? Mevkii { get; set; }
        public string? AdaNo { get; set; }
        public string? ParselNo { get; set; }
        public string? Pafta { get; set; }
        public string? Nitelik { get; set; }
        public string? ZeminKmdurum { get; set; }
        public string? Ozet { get; set; }

        public double AreaSquareMeters { get; set; }
        public string? AreaFormatted { get; set; }

        public double CenterLat { get; set; }
        public double CenterLng { get; set; }

        public string? GeoJson { get; set; }

        public string? Ada => AdaNo;
        public string? Parsel => ParselNo;
        public double Alan => AreaSquareMeters;
        public double CenterLatitude => CenterLat;
        public double CenterLongitude => CenterLng;
    }

    public class ApplyTkgmDto
    {
        public int ProjectId { get; set; }
        public string? Ada { get; set; }
        public string? Parsel { get; set; }
        public double? TotalLandArea { get; set; }
        public double? Area { get => TotalLandArea; set => TotalLandArea = value; }
        public string? Pafta { get; set; }
        public string? Nitelik { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? CenterLatitude { get => Latitude; set => Latitude = value; }
        public double? CenterLongitude { get => Longitude; set => Longitude = value; }
        public string? GeoJson { get; set; }
    }
}
