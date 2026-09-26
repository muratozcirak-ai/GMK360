using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using GMK360.Core.Entities.B2b;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public partial class AdminController : Controller
    {
        public IActionResult B2bMarketplace()
        {
            return View();
        }

        public async Task<IActionResult> B2bList(string type)
        {
            var query = _context.B2bCompanies
                .Include(c => c.AddedByAgency)
                .Include(c => c.CompanyCategories)
                    .ThenInclude(cc => cc.DefinitionValue)
                .Include(c => c.Branches)
                .Include(c => c.Contacts)
                .AsQueryable();

            string pageTitle = "";
            string categoryCode = "B2BSectors";
            
            if (type == "usta")
            {
                query = query.Where(c => c.IsSubcontractor == true);
                pageTitle = "Usta & Taşeron Ekipler";
                categoryCode = "B2BUstaSectors";
            }
                        else if (type == "tedarikci")
            {
                query = query.Where(c => c.IsSupplier == true);
                pageTitle = "Malzeme Tedarikçileri & Nalburlar";
                categoryCode = "B2BMaterialCategories";
            }
            else
            {
                type = "firma";
                query = query.Where(c => c.IsEngineering == true);
                pageTitle = "Kurumsal & Profesyonel Hizmetler";
            }

            ViewBag.ListType = type;
            ViewBag.PageTitle = pageTitle;
            
            ViewBag.Categories = await _context.DefinitionValues
                .Include(v => v.Category)
                .Where(v => v.Category.SystemCode == categoryCode)
                .OrderBy(v => v.Order)
                .ToListAsync();
                
            ViewBag.Cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();

            var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> AddB2bCompany(string name, int legalStatus, string tcKimlik, string taxOffice, string taxNumber, bool isEnterprise, string phone, int cityId, int districtId, int neighborhoodId, int[] categoryIds, string listType)
        {
            var company = new B2bCompany
            {
                Name = name ?? "İsimsiz Kayıt",
                LegalStatus = (LegalEntityType)legalStatus,
                IsEnterprise = isEnterprise,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsVerified = false,
                Rating = 0,
                AddedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            };

            // Hukuki Statüye Göre Vergi / TC
            if (company.LegalStatus == LegalEntityType.Individual)
            {
                company.TaxNumber = tcKimlik; // TC'yi TaxNumber'da tutuyoruz
            }
            else
            {
                company.TaxOffice = taxOffice;
                company.TaxNumber = taxNumber;
            }

            // Hangi Sayfadan Eklendiğine Göre Varsayılan Rolleri Ata
            if (listType == "usta") company.IsSubcontractor = true;
            if (listType == "tedarikci") company.IsSupplier = true;
            if (listType == "firma") company.IsEngineering = true;

            // İletişim
            if (!string.IsNullOrEmpty(phone))
            {
                company.Contacts.Add(new B2bContact 
                { 
                    FullName = name ?? "Bilinmiyor", 
                    MobilePhone = phone,
                    DepartmentOrRole = "Ana İletişim",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                });
            }

            // Lokasyon (İl, İlçe, Mahalle)
            if (cityId > 0)
            {
                var city = await _context.Cities.FirstOrDefaultAsync(c => c.Id == cityId);
                var district = districtId > 0 ? await _context.Districts.FirstOrDefaultAsync(d => d.Id == districtId) : null;
                var neighborhood = neighborhoodId > 0 ? await _context.Neighborhoods.FirstOrDefaultAsync(n => n.Id == neighborhoodId) : null;
                
                var branch = new B2bBranch 
                { 
                    BranchName = "Ana Hizmet Bölgesi", 
                    City = city?.Name,
                    District = district?.Name,
                    Address = neighborhood?.Name, // Mahalle adını geçici olarak adres kolonuna yazıyoruz (veya modelde Neighborhood kolonu varsa oraya)
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                company.Branches.Add(branch);
            }

            // Sektör / Ustalık Kategorileri
            if (categoryIds != null && categoryIds.Length > 0)
            {
                foreach (var catId in categoryIds)
                {
                    company.CompanyCategories.Add(new B2bCompanyCategory { DefinitionValueId = catId });
                }
            }

            _context.B2bCompanies.Add(company);
            await _context.SaveChangesAsync();

            return RedirectToAction("B2bList", new { type = listType });
        }
    }
}
