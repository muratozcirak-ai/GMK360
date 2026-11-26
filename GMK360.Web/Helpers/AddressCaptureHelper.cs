using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Helpers
{
    public sealed class StructuredAddressInput
    {
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        public string? StreetName { get; set; }
        public string? BuildingNo { get; set; }
        public string? ApartmentNo { get; set; }
    }

    public sealed class ResolvedAddressResult
    {
        public bool IsValid { get; set; }
        public int? CityId { get; set; }
        public int? DistrictId { get; set; }
        public int? NeighborhoodId { get; set; }
        public int? StreetId { get; set; }
        public string? StreetName { get; set; }
        public string? BuildingNo { get; set; }
        public string? ApartmentNo { get; set; }
        public string FormattedAddress { get; set; } = string.Empty;
    }

    public static class AddressCaptureHelper
    {
        public static async Task<ResolvedAddressResult> ResolveAndEnrichAsync(
            ApplicationDbContext context,
            StructuredAddressInput input)
        {
            if (input.CityId is null or <= 0 || input.DistrictId is null or <= 0 || input.NeighborhoodId is null or <= 0)
            {
                return new ResolvedAddressResult { IsValid = false };
            }

            var city = await context.Cities.AsNoTracking().FirstOrDefaultAsync(c => c.Id == input.CityId.Value);
            var district = await context.Districts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == input.DistrictId.Value && d.CityId == input.CityId.Value);
            var neighborhood = await context.Neighborhoods.AsNoTracking().FirstOrDefaultAsync(n => n.Id == input.NeighborhoodId.Value && n.DistrictId == input.DistrictId.Value);
            if (city == null || district == null || neighborhood == null)
            {
                return new ResolvedAddressResult { IsValid = false };
            }

            int? resolvedStreetId = input.StreetId is > 0 ? input.StreetId : null;
            string? resolvedStreetName = string.IsNullOrWhiteSpace(input.StreetName) ? null : input.StreetName.Trim();

            if (resolvedStreetId.HasValue)
            {
                var st = await context.Streets.AsNoTracking().FirstOrDefaultAsync(s => s.Id == resolvedStreetId.Value);
                if (st != null) resolvedStreetName = st.Name;
            }
            else if (!string.IsNullOrWhiteSpace(resolvedStreetName))
            {
                var existingSt = await context.Streets.FirstOrDefaultAsync(s =>
                    s.NeighborhoodId == neighborhood.Id && s.Name.ToLower() == resolvedStreetName.ToLower());
                if (existingSt != null)
                {
                    resolvedStreetId = existingSt.Id;
                    resolvedStreetName = existingSt.Name;
                }
                else
                {
                    var createdSt = new Street
                    {
                        NeighborhoodId = neighborhood.Id,
                        Name = resolvedStreetName,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Streets.Add(createdSt);
                    await context.SaveChangesAsync();
                    resolvedStreetId = createdSt.Id;
                }
            }

            if (string.IsNullOrWhiteSpace(resolvedStreetName) || string.IsNullOrWhiteSpace(input.BuildingNo))
            {
                return new ResolvedAddressResult { IsValid = false };
            }

            string cleanBuildingNo = input.BuildingNo.Trim();
            string? cleanAptNo = string.IsNullOrWhiteSpace(input.ApartmentNo) ? null : input.ApartmentNo.Trim();

            // Merkezi AdresLokasyon tablosunu da besle (tekrar yoksa ekle)
            bool hasLoc = await context.AddressLocations.AnyAsync(a =>
                a.City == city.Name && a.District == district.Name && a.Neighborhood == neighborhood.Name);
            if (!hasLoc)
            {
                context.AddressLocations.Add(new AddressLocation
                {
                    City = city.Name,
                    District = district.Name,
                    Neighborhood = neighborhood.Name,
                    ComplexName = $"{resolvedStreetName} No:{cleanBuildingNo}",
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

            string formatted = $"{neighborhood.Name}, {resolvedStreetName} No: {cleanBuildingNo}"
                + (!string.IsNullOrWhiteSpace(cleanAptNo) ? $" D: {cleanAptNo}" : "")
                + $", {district.Name} / {city.Name}";

            return new ResolvedAddressResult
            {
                IsValid = true,
                CityId = city.Id,
                DistrictId = district.Id,
                NeighborhoodId = neighborhood.Id,
                StreetId = resolvedStreetId,
                StreetName = resolvedStreetName,
                BuildingNo = cleanBuildingNo,
                ApartmentNo = cleanAptNo,
                FormattedAddress = formatted
            };
        }

        public static void ApplyToUser(ApplicationUser user, ResolvedAddressResult res)
        {
            if (user == null || !res.IsValid) return;
            user.AddressCityId = res.CityId;
            user.AddressDistrictId = res.DistrictId;
            user.AddressNeighborhoodId = res.NeighborhoodId;
            user.AddressStreetId = res.StreetId;
            user.AddressStreetName = res.StreetName;
            user.AddressBuildingNo = res.BuildingNo;
            user.AddressApartmentNo = res.ApartmentNo;
            user.Address = res.FormattedAddress;
        }

        public static void ApplyToStaff(BuildingStaff staff, ResolvedAddressResult res)
        {
            if (staff == null || !res.IsValid) return;
            staff.AddressCityId = res.CityId;
            staff.AddressDistrictId = res.DistrictId;
            staff.AddressNeighborhoodId = res.NeighborhoodId;
            staff.AddressStreetId = res.StreetId;
            staff.AddressStreetName = res.StreetName;
            staff.AddressBuildingNo = res.BuildingNo;
            staff.AddressApartmentNo = res.ApartmentNo;
            staff.Address = res.FormattedAddress;
        }
    }
}
