using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using GMK360.Core.DTOs;
using GMK360.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace GMK360.Web.Services
{
    public class TkgmService : ITkgmService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TkgmService> _logger;
        private const string BaseApiUrl = "https://cbsapi.tkgm.gov.tr/megsiswebapi.v3.1/api/";

        public TkgmService(HttpClient httpClient, ILogger<TkgmService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(BaseApiUrl);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(15);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://parselsorgu.tkgm.gov.tr/");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://parselsorgu.tkgm.gov.tr");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<TkgmParcelResultDto> GetParcelByCoordinatesAsync(double latitude, double longitude)
        {
            try
            {
                var latStr = latitude.ToString("F6", CultureInfo.InvariantCulture);
                var lonStr = longitude.ToString("F6", CultureInfo.InvariantCulture);
                var endpoint = $"parsel/{latStr}/{lonStr}/";

                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("TKGM MEGSIS API returned status {StatusCode} for ({Lat}, {Lon})", response.StatusCode, latStr, lonStr);
                    return new TkgmParcelResultDto
                    {
                        Success = false,
                        Message = $"TKGM Servisi yanıt vermedi (HTTP {(int)response.StatusCode})."
                    };
                }

                return ParseParcelResponse(content, latitude, longitude);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying TKGM parcel for coordinates ({Lat}, {Lon})", latitude, longitude);
                return new TkgmParcelResultDto
                {
                    Success = false,
                    Message = "TKGM servisine bağlanırken bir hata oluştu: " + ex.Message
                };
            }
        }

        public async Task<TkgmParcelResultDto> GetParcelByAdaParselAsync(long mahalleId, string ada, string parsel)
        {
            try
            {
                var endpoint = $"parsel/{mahalleId}/{ada.Trim()}/{parsel.Trim()}/";

                var response = await _httpClient.GetAsync(endpoint);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("TKGM MEGSIS API returned status {StatusCode} for MahalleId:{MahalleId} Ada:{Ada} Parsel:{Parsel}", response.StatusCode, mahalleId, ada, parsel);
                    return new TkgmParcelResultDto
                    {
                        Success = false,
                        Message = $"TKGM Servisi yanıt vermedi (HTTP {(int)response.StatusCode})."
                    };
                }

                return ParseParcelResponse(content, 0, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying TKGM parcel for MahalleId:{MahalleId} Ada:{Ada} Parsel:{Parsel}", mahalleId, ada, parsel);
                return new TkgmParcelResultDto
                {
                    Success = false,
                    Message = "TKGM servisine bağlanırken bir hata oluştu: " + ex.Message
                };
            }
        }

        public async Task<List<TkgmAdministrativeUnitDto>> GetProvincesAsync()
        {
            return await QueryAdministrativeUnitsAsync("idariYapi/ilListe/");
        }

        public async Task<List<TkgmAdministrativeUnitDto>> GetDistrictsAsync(int provinceId)
        {
            return await QueryAdministrativeUnitsAsync($"idariYapi/ilceListe/{provinceId}/");
        }

        public async Task<List<TkgmAdministrativeUnitDto>> GetNeighborhoodsAsync(int districtId)
        {
            return await QueryAdministrativeUnitsAsync($"idariYapi/mahalleListe/{districtId}/");
        }

        private async Task<List<TkgmAdministrativeUnitDto>> QueryAdministrativeUnitsAsync(string endpoint)
        {
            var list = new List<TkgmAdministrativeUnitDto>();
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode) return list;

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in features.EnumerateArray())
                    {
                        if (item.TryGetProperty("properties", out var props))
                        {
                            long id = 0;
                            string text = string.Empty;

                            if (props.TryGetProperty("id", out var idProp))
                            {
                                if (idProp.ValueKind == JsonValueKind.Number) id = idProp.GetInt64();
                                else if (idProp.ValueKind == JsonValueKind.String && long.TryParse(idProp.GetString(), out var parsedId)) id = parsedId;
                            }

                            if (props.TryGetProperty("text", out var textProp))
                            {
                                text = textProp.GetString() ?? string.Empty;
                            }

                            if (id > 0 && !string.IsNullOrWhiteSpace(text))
                            {
                                list.Add(new TkgmAdministrativeUnitDto { Id = id, Text = text });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying TKGM administrative units for endpoint {Endpoint}", endpoint);
            }
            return list;
        }

        private TkgmParcelResultDto ParseParcelResponse(string content, double defaultLat, double defaultLng)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // Check for error message
                if (root.TryGetProperty("Message", out var msgProp))
                {
                    return new TkgmParcelResultDto
                    {
                        Success = false,
                        Message = msgProp.GetString() ?? "Parsel bulunamadı."
                    };
                }

                if (!root.TryGetProperty("type", out var typeProp) || typeProp.GetString() != "Feature")
                {
                    return new TkgmParcelResultDto
                    {
                        Success = false,
                        Message = "Beklenmeyen veri formatı alındı."
                    };
                }

                var result = new TkgmParcelResultDto
                {
                    Success = true,
                    GeoJson = content
                };

                // Properties
                if (root.TryGetProperty("properties", out var props))
                {
                    result.IlAd = GetStringProp(props, "ilAd");
                    result.IlceAd = GetStringProp(props, "ilceAd");
                    result.MahalleAd = GetStringProp(props, "mahalleAd");
                    result.Mevkii = GetStringProp(props, "mevkii");
                    result.AdaNo = GetStringProp(props, "adaNo");
                    result.ParselNo = GetStringProp(props, "parselNo");
                    result.Pafta = GetStringProp(props, "pafta");
                    result.Nitelik = GetStringProp(props, "nitelik");
                    result.ZeminKmdurum = GetStringProp(props, "zeminKmdurum");
                    result.Ozet = GetStringProp(props, "ozet");

                    if (props.TryGetProperty("ilId", out var ilId) && ilId.ValueKind == JsonValueKind.Number)
                        result.IlId = ilId.GetInt32();
                    if (props.TryGetProperty("ilceId", out var ilceId) && ilceId.ValueKind == JsonValueKind.Number)
                        result.IlceId = ilceId.GetInt32();
                    if (props.TryGetProperty("mahalleId", out var mahId))
                    {
                        if (mahId.ValueKind == JsonValueKind.Number) result.MahalleId = mahId.GetInt64();
                        else if (long.TryParse(mahId.GetString(), out var mid)) result.MahalleId = mid;
                    }

                    // Alan parsing: e.g. "192,00" or "1.500,50"
                    var alanRaw = GetStringProp(props, "alan");
                    if (!string.IsNullOrWhiteSpace(alanRaw))
                    {
                        result.AreaFormatted = alanRaw + " m²";
                        var cleanStr = alanRaw.Trim().Replace(".", "").Replace(",", ".");
                        if (double.TryParse(cleanStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var areaVal))
                        {
                            result.AreaSquareMeters = areaVal;
                        }
                    }
                }

                // Compute bounding box / center lat/lng from geometry
                double minLat = double.MaxValue, maxLat = double.MinValue;
                double minLng = double.MaxValue, maxLng = double.MinValue;
                bool hasCoords = false;

                if (root.TryGetProperty("geometry", out var geom) && geom.TryGetProperty("coordinates", out var coords))
                {
                    ExtractMinMaxCoords(coords, ref minLat, ref maxLat, ref minLng, ref maxLng, ref hasCoords);
                }

                if (hasCoords)
                {
                    result.CenterLat = (minLat + maxLat) / 2.0;
                    result.CenterLng = (minLng + maxLng) / 2.0;
                }
                else
                {
                    result.CenterLat = defaultLat;
                    result.CenterLng = defaultLng;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing TKGM parcel response");
                return new TkgmParcelResultDto
                {
                    Success = false,
                    Message = "Parsel verisi ayrıştırılırken hata oluştu: " + ex.Message
                };
            }
        }

        private static string? GetStringProp(JsonElement element, string propName)
        {
            if (element.TryGetProperty(propName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.String) return prop.GetString();
                if (prop.ValueKind == JsonValueKind.Number) return prop.ToString();
            }
            return null;
        }

        private static void ExtractMinMaxCoords(JsonElement element, ref double minLat, ref double maxLat, ref double minLng, ref double maxLng, ref bool hasCoords)
        {
            if (element.ValueKind == JsonValueKind.Array)
            {
                // Check if this array is a point [lng, lat]
                if (element.GetArrayLength() >= 2 &&
                    element[0].ValueKind == JsonValueKind.Number &&
                    element[1].ValueKind == JsonValueKind.Number)
                {
                    double lng = element[0].GetDouble();
                    double lat = element[1].GetDouble();

                    if (lat < minLat) minLat = lat;
                    if (lat > maxLat) maxLat = lat;
                    if (lng < minLng) minLng = lng;
                    if (lng > maxLng) maxLng = lng;
                    hasCoords = true;
                }
                else
                {
                    foreach (var child in element.EnumerateArray())
                    {
                        ExtractMinMaxCoords(child, ref minLat, ref maxLat, ref minLng, ref maxLng, ref hasCoords);
                    }
                }
            }
        }
    }
}
