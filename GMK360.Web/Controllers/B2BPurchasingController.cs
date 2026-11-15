using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Core.Entities.B2B;
using GMK360.Core.Entities.Identity;
using GMK360.Core.Entities.Construction;
using System;

namespace GMK360.Web.Controllers
{
    [Authorize]
    public class B2BPurchasingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GMK360.Web.Services.B2BSupplierSyncService _supplierSyncService;

        public B2BPurchasingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, GMK360.Web.Services.B2BSupplierSyncService supplierSyncService)
        {
            _context = context;
            _userManager = userManager;
            _supplierSyncService = supplierSyncService;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
                if (consultant != null && consultant.AgencyId > 0)
                    return consultant.AgencyId;
            }
            var agencyClaim = User.FindFirst("AgencyId")?.Value;
            if (int.TryParse(agencyClaim, out int parsedAgencyId) && parsedAgencyId > 0)
                return parsedAgencyId;

            var activeProjAgency = await _context.ConstructionProjects
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.AgencyId > 0)
                .OrderByDescending(p => p.Id)
                .Select(p => (int?)p.AgencyId)
                .FirstOrDefaultAsync();
            if (activeProjAgency.HasValue && activeProjAgency.Value > 0)
                return activeProjAgency.Value;

            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) return 1;
            return null;
        }

        public async Task<IActionResult> Index()
        {
            if (!await GMK360.Web.Helpers.StaffPermissionHelper.HasUserPermissionAsync(_context, User, Request, GMK360.Web.Helpers.StaffPermissionHelper.PermPurchasing))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

                var query = _context.B2BQuoteRequests
                    .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                    .AsQueryable();

                query = query.Where(q => q.RequesterAgencyId == agencyId);

                var quotes = await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
                if (agencyId.HasValue) 
                {
                    ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId.Value).OrderBy(c => c.Name).ToListAsync();
                    ViewBag.Projects = await _context.ConstructionProjects.Where(p => p.AgencyId == agencyId.Value && p.Status != GMK360.Core.Entities.Construction.ProjectStatus.Tamamlandi_Teslim && p.Status != GMK360.Core.Entities.Construction.ProjectStatus.Anlasma_Olmayan_Iptal).OrderBy(p => p.Name).ToListAsync();
                }
                return View(quotes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sistem Hatası: " + ex.Message;
                return View(new System.Collections.Generic.List<B2BQuoteRequest>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuote(string title, string description, DateTime? deadline, int[] materialCatalogIds, decimal[] quantities, int? projectId)
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();
                
                var user = await _userManager.GetUserAsync(User);

                string finalTitle = title;
                if (string.IsNullOrWhiteSpace(finalTitle) && materialCatalogIds != null && materialCatalogIds.Length > 0)
                {
                    var firstCat = await _context.MaterialCatalogs.FindAsync(materialCatalogIds[0]);
                    if (firstCat != null) finalTitle = "Toplu Malzeme Alımı (" + firstCat.Name + " vb.)";
                }

                var quote = new B2BQuoteRequest
                {
                    RequesterAgencyId = agencyId.Value,
                    RequesterUserId = user?.Id ?? "",
                    SourceModule = (projectId.HasValue && projectId.Value > 0) ? "ConstructionProject" : "Direct",
                    SourceReferenceId = projectId ?? 0,
                    Title = finalTitle ?? "İsimsiz Alım",
                    Description = description ?? "",
                    Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow
                };

                _context.B2BQuoteRequests.Add(quote);
                await _context.SaveChangesAsync();

                if (materialCatalogIds != null && quantities != null)
                {
                    for(int i = 0; i < materialCatalogIds.Length; i++)
                    {
                        var catId = materialCatalogIds[i];
                        var qty = i < quantities.Length ? quantities[i] : 0;
                        
                        if (catId > 0 && qty > 0)
                        {
                            var quoteItem = new B2BQuoteItem
                            {
                                B2BQuoteRequestId = quote.Id,
                                MaterialCatalogId = catId,
                                Quantity = qty,
                                Description = ""
                            };
                            _context.B2BQuoteItems.Add(quoteItem);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Alım Emri / İhale başarıyla oluşturuldu!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Alım kaydı oluşturulurken hata oluştu: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "");
                return RedirectToAction("Index");
            }
        }
        
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuoteItem(int quoteRequestId, int materialCatalogId, decimal quantity, string description)
        {
            var quote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == quoteRequestId);
            if (quote == null) return NotFound();

            var agencyId = await GetUserAgencyIdAsync();
            if (quote.RequesterAgencyId != agencyId && !User.IsInRole("Admin")) return Unauthorized();

            var item = new B2BQuoteItem
            {
                B2BQuoteRequestId = quoteRequestId,
                MaterialCatalogId = materialCatalogId,
                Quantity = quantity,
                Description = description ?? ""
            };
            
            _context.B2BQuoteItems.Add(item);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Kalem başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = quoteRequestId });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveQuoteItem(int itemId)
        {
            var item = await _context.B2BQuoteItems.Include(i => i.QuoteRequest).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return NotFound();
            
            var agencyId = await GetUserAgencyIdAsync();
            if (item.QuoteRequest.RequesterAgencyId != agencyId && !User.IsInRole("Admin")) return Unauthorized();

            _context.B2BQuoteItems.Remove(item);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Kalem listeden çıkarıldı.";
            return RedirectToAction(nameof(Details), new { id = item.B2BQuoteRequestId });
        }

        public async Task<IActionResult> Details(int id)
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

                var quote = await _context.B2BQuoteRequests
                    .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                    .Include(q => q.Invites).ThenInclude(i => i.InviteItems).ThenInclude(ii => ii.QuoteItem).ThenInclude(qi => qi.MaterialCatalog)
                    .Include(q => q.Items).ThenInclude(i => i.MaterialCatalog)
                    .FirstOrDefaultAsync(q => q.Id == id);
                    
                if (quote == null || (!User.IsInRole("Admin") && quote.RequesterAgencyId != agencyId)) 
                    return NotFound();

                var currentUserId = _userManager.GetUserId(User) ?? "";
                await _supplierSyncService.AutoSyncExistingNetworkContactsAsync(quote.RequesterAgencyId, currentUserId);

                GMK360.Core.Entities.Construction.ConstructionProject project = null;
                if (quote.SourceModule == "PhaseZeroDocument")
                {
                    var doc = await _context.ProjectLegalDocuments
                        .Include(d => d.ConstructionProject)
                        .FirstOrDefaultAsync(d => d.Id == quote.SourceReferenceId);
                    project = doc?.ConstructionProject;
                }
                else if (quote.SourceModule == "ConstructionProject" && quote.SourceReferenceId > 0)
                {
                    project = await _context.ConstructionProjects.FindAsync(quote.SourceReferenceId);
                }
                ViewBag.Project = project;
                if (quote.SourceModule == "PhaseZeroDocument") { ViewBag.LegalDocument = await _context.ProjectLegalDocuments.FirstOrDefaultAsync(d => d.Id == quote.SourceReferenceId); }
                ViewBag.PhonebookContacts = await _context.AgencyPhonebooks.Where(p => p.AgencyId == quote.RequesterAgencyId).OrderBy(p => p.Name).ToListAsync();
                ViewBag.B2bStatusMap = (await _context.B2bCompanies.AsNoTracking().ToListAsync()).GroupBy(c => (c.Name ?? "").Trim().ToLowerInvariant()).Where(g => !string.IsNullOrEmpty(g.Key)).ToDictionary(g => g.Key, g => g.First());

                byte contactType = _supplierSyncService.DetermineContactType(quote.Title);
                var recommendedQuery = _context.B2bCompanies
                    .Include(c => c.Contacts)
                    .Include(c => c.CompanyCategories)
                        .ThenInclude(cc => cc.DefinitionValue)
                    .AsQueryable();

                if (contactType == 8 || contactType == 4)
                {
                    recommendedQuery = recommendedQuery.Where(c => c.IsEngineering);
                }
                else if (contactType == 3)
                {
                    recommendedQuery = recommendedQuery.Where(c => c.IsSupplier);
                }
                else
                {
                    recommendedQuery = recommendedQuery.Where(c => c.IsSubcontractor);
                }

                var invitedCompanyNames = quote.Invites.Where(i => i.NetworkContact != null).Select(i => i.NetworkContact.CompanyName.Trim().ToLower()).ToList();
                var uninvitedQuery = recommendedQuery.Where(c => !invitedCompanyNames.Contains(c.Name.Trim().ToLower()));
                var totalEligibleCount = await uninvitedQuery.CountAsync();
                var recommended = await uninvitedQuery.Take(6).ToListAsync();

                ViewBag.TotalEligibleMarketCompaniesCount = totalEligibleCount;
                ViewBag.RecommendedCompanies = recommended;
                    
                ViewBag.Warehouses = await _context.Warehouses.Where(w => w.AgencyId == quote.RequesterAgencyId).ToListAsync();
                ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == quote.RequesterAgencyId).OrderBy(c => c.Name).ToListAsync();
                ViewBag.Consultants = await _context.Set<GMK360.Core.Entities.AgencyConsultant>()
                    .Include(c => c.User)
                    .Where(c => c.AgencyId == quote.RequesterAgencyId && c.User != null)
                    .ToListAsync();
                
                return View(quote);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Detaylar yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAndComplete(
            int quoteRequestId, 
            int inviteId, 
            int? warehouseId = null, 
            string? assignedUserId = null, 
            int[]? inviteItemIds = null, 
            decimal[]? acceptedQuantities = null,
            bool isPaidUpfront = false,
            DateTime? dueDate = null,
            string? approvalMode = "ContractOnly")
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                var quote = await _context.B2BQuoteRequests
                    .Include(q => q.Items)
                    .ThenInclude(i => i.MaterialCatalog)
                    .FirstOrDefaultAsync(q => q.Id == quoteRequestId);
                    
                if (quote == null || quote.RequesterAgencyId != agencyId) return NotFound();

                var invite = await _context.B2BQuoteInvites
                    .Include(i => i.NetworkContact)
                    .Include(i => i.InviteItems)
                    .FirstOrDefaultAsync(i => i.Id == inviteId && i.QuoteRequestId == quoteRequestId);

                if (invite == null) return NotFound();

                quote.Status = "Completed";
                invite.Status = QuoteInviteStatus.Accepted;

                var receipt = new InventoryReceipt
                {
                    AgencyId = agencyId.Value,
                    WarehouseId = warehouseId ?? 0,
                    B2BQuoteRequestId = quote.Id,
                    SupplierName = invite.NetworkContact?.CompanyName ?? "Bilinmiyor",
                    DocumentNumber = "B2B-" + quote.Id.ToString(),
                    ReceiptDate = DateTime.UtcNow,
                    Notes = "B2B Siparişi. Proje/Şantiye teslimatı bekleniyor.",
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow,
                    AssignedUserId = assignedUserId ?? "",
                    Items = new System.Collections.Generic.List<InventoryReceiptItem>()
                };

                decimal actualInvoiceAmount = 0;

                if (invite.InviteItems != null && inviteItemIds != null && acceptedQuantities != null)
                {
                    for (int i = 0; i < inviteItemIds.Length; i++)
                    {
                        var invItem = invite.InviteItems.FirstOrDefault(x => x.Id == inviteItemIds[i]);
                        var qty = acceptedQuantities[i];
                        
                        if (invItem != null && qty > 0)
                        {
                            var reqItem = quote.Items.FirstOrDefault(x => x.Id == invItem.B2BQuoteItemId);
                            if (reqItem != null)
                            {
                                receipt.Items.Add(new InventoryReceiptItem
                                {
                                    MaterialCatalogId = reqItem.MaterialCatalogId,
                                    Quantity = qty,
                                    UnitPrice = invItem.OfferedUnitPrice,
                                    CreatedAt = DateTime.UtcNow
                                });
                                
                                actualInvoiceAmount += (invItem.OfferedUnitPrice * qty);
                            }
                        }
                    }
                }

                bool isContractOnlyMode = string.Equals(approvalMode, "ContractOnly", StringComparison.OrdinalIgnoreCase) && quote.SourceModule != "PhaseZeroDocument";
                if (string.Equals(approvalMode, "Pesin", StringComparison.OrdinalIgnoreCase)) isPaidUpfront = true;

                if (!isContractOnlyMode && warehouseId.HasValue && warehouseId.Value > 0) _context.InventoryReceipts.Add(receipt);
                
                if (actualInvoiceAmount == 0 && invite.OfferedPrice.HasValue && invite.OfferedPrice.Value > 0)
                {
                    actualInvoiceAmount = invite.OfferedPrice.Value;
                }

                int? openedCurrentAccountId = null;

                if (invite.NetworkContact != null && actualInvoiceAmount > 0)
                {
                    int? projId = (quote.SourceModule == "ConstructionProject" && quote.SourceReferenceId > 0) ? quote.SourceReferenceId : null;
                    string projName = "";
                    if (projId.HasValue)
                    {
                        var pObj = await _context.ConstructionProjects.FindAsync(projId.Value);
                        projName = pObj?.Name ?? "";
                    }
                    if (quote.SourceModule == "PhaseZeroDocument")
                    {
                        var doc = await _context.ProjectLegalDocuments.FindAsync(quote.SourceReferenceId);
                        projId = doc?.ConstructionProjectId;
                        if (doc != null) { doc.EstimatedCost = actualInvoiceAmount; doc.ActualCost = actualInvoiceAmount; doc.InstitutionContact = invite.NetworkContact?.CompanyName ?? doc.InstitutionContact; doc.Status = "Tamamlandı"; }
                    }

                    var syncResult = await _supplierSyncService.EnsureSupplierSynchronizedAsync(
                        agencyId.Value,
                        _userManager.GetUserId(User) ?? "",
                        invite.NetworkContact.CompanyName,
                        invite.NetworkContact.ContactPerson,
                        invite.NetworkContact.PhoneNumber,
                        invite.NetworkContact.Email,
                        quote.Title);

                    var currentAccount = await _supplierSyncService.EnsureSupplierCurrentAccountOnlyAsync(
                        agencyId.Value,
                        syncResult.PhonebookContact.Id);
                    openedCurrentAccountId = currentAccount.Id;

                    // Anlaşılan paket ve kalem birim fiyatlarını Firma Carisine (supplier_contracts.json) kaydet
                    var contractItems = new System.Collections.Generic.List<GMK360.Web.Services.SupplierContractItemDto>();
                    if (invite.InviteItems != null && invite.InviteItems.Any())
                    {
                        for (int idx = 0; idx < invite.InviteItems.Count; idx++)
                        {
                            var invItem = invite.InviteItems.ElementAt(idx);
                            var reqItem = quote.Items?.FirstOrDefault(x => x.Id == invItem.B2BQuoteItemId);
                            decimal agreedQty = reqItem?.Quantity ?? 1m;
                            if (inviteItemIds != null && acceptedQuantities != null)
                            {
                                int pos = Array.IndexOf(inviteItemIds, invItem.Id);
                                if (pos >= 0 && pos < acceptedQuantities.Length && acceptedQuantities[pos] > 0)
                                    agreedQty = acceptedQuantities[pos];
                            }
                            contractItems.Add(new GMK360.Web.Services.SupplierContractItemDto
                            {
                                MaterialCatalogId = reqItem?.MaterialCatalogId ?? 0,
                                MaterialName = reqItem?.MaterialCatalog?.Name ?? reqItem?.Description ?? "Malzeme Kalemi",
                                Unit = reqItem?.MaterialCatalog?.DefaultUnit ?? "Adet",
                                AgreedQuantity = agreedQty,
                                OrderedQuantity = isContractOnlyMode ? 0m : agreedQty,
                                AgreedUnitPrice = invItem.OfferedUnitPrice
                            });
                        }
                    }
                    else if (quote.Items != null && quote.Items.Any())
                    {
                        int itemCount = quote.Items.Count;
                        foreach (var reqItem in quote.Items)
                        {
                            decimal qty = reqItem.Quantity > 0 ? reqItem.Quantity : 1m;
                            decimal estUnitPrice = itemCount == 1 ? Math.Round(actualInvoiceAmount / qty, 2) : 0m;
                            contractItems.Add(new GMK360.Web.Services.SupplierContractItemDto
                            {
                                MaterialCatalogId = reqItem.MaterialCatalogId,
                                MaterialName = reqItem.MaterialCatalog?.Name ?? reqItem.Description ?? "Malzeme Kalemi",
                                Unit = reqItem.MaterialCatalog?.DefaultUnit ?? "Adet",
                                AgreedQuantity = qty,
                                OrderedQuantity = isContractOnlyMode ? 0m : qty,
                                AgreedUnitPrice = estUnitPrice
                            });
                        }
                    }

                    var allContracts = _supplierSyncService.LoadAgencySupplierContracts(agencyId.Value);
                    var existingContract = allContracts.FirstOrDefault(c => c.QuoteRequestId == quote.Id && c.InviteId == invite.Id);
                    if (existingContract == null)
                    {
                        existingContract = new GMK360.Web.Services.SupplierContractPackageDto
                        {
                            ContractId = Guid.NewGuid().ToString("N"),
                            AgencyId = agencyId.Value,
                            SupplierCurrentAccountId = currentAccount.Id,
                            PhonebookContactId = syncResult.PhonebookContact.Id,
                            SupplierName = invite.NetworkContact.CompanyName ?? "",
                            QuoteRequestId = quote.Id,
                            InviteId = invite.Id,
                            ProjectId = projId,
                            ProjectName = projName,
                            PackageTitle = quote.Title ?? "Malzeme Tedarik Anlaşması",
                            TotalAgreedAmount = actualInvoiceAmount,
                            OrderedTotalAmount = isContractOnlyMode ? 0m : actualInvoiceAmount,
                            IsVatIncluded = invite.IsVatIncludedGlobally,
                            ContractTermsNote = invite.OfferNotes ?? "",
                            ApprovedAt = DateTime.UtcNow,
                            Items = contractItems
                        };
                        allContracts.Add(existingContract);
                    }
                    else
                    {
                        existingContract.SupplierCurrentAccountId = currentAccount.Id;
                        existingContract.TotalAgreedAmount = actualInvoiceAmount;
                        existingContract.OrderedTotalAmount = isContractOnlyMode ? 0m : actualInvoiceAmount;
                        existingContract.Items = contractItems;
                    }
                    _supplierSyncService.SaveAgencySupplierContracts(agencyId.Value, allContracts);

                    if (!isContractOnlyMode)
                    {
                        await _supplierSyncService.OpenOrUpdateCurrentAccountAsync(
                            agencyId.Value,
                            syncResult.PhonebookContact.Id,
                            projId,
                            actualInvoiceAmount,
                            $"B2B İhale Onayı #{quote.Id} - {quote.Title}",
                            receipt.DocumentNumber,
                            _userManager.GetUserId(User) ?? "",
                            isPaidUpfront: isPaidUpfront,
                            dueDate: dueDate);

                        // Şantiye Kara Defterine Teslimat & İmalat Takibi Görevi Ekle
                        var targetDeliveryDate = dueDate ?? DateTime.UtcNow.AddDays(7);
                        var paymentTermsText = isPaidUpfront ? "Peşin Ödendi" : $"Vadeli (Vade: {targetDeliveryDate:dd.MM.yyyy})";
                        await _supplierSyncService.CreateDeliveryAgendaRecordAsync(
                            agencyId.Value,
                            projId,
                            syncResult.PhonebookContact.Id,
                            invite.NetworkContact.CompanyName,
                            quote.Title,
                            actualInvoiceAmount,
                            paymentTermsText,
                            targetDeliveryDate);
                    }
                }
                
                await _context.SaveChangesAsync();

                if (openedCurrentAccountId.HasValue && quote.SourceModule != "PhaseZeroDocument")
                {
                    TempData["SuccessMessage"] = isContractOnlyMode
                        ? $"🤝 {invite.NetworkContact?.CompanyName} ile {actualInvoiceAmount:N2} ₺ tutarındaki '{quote.Title}' anlaşması onaylandı ve Firma Carisi açıldı! Toplu anlaşma tutarı (beklenen çıkış) ve tüm birim fiyatlar cariye işlendi. Aşağıdan ödeme şartlarını belirleyebilir ve birim fiyatlarla doğrudan sipariş verebilirsiniz."
                        : $"✅ Teklif onaylandı! {actualInvoiceAmount:N2} ₺ firma cari hesabına ve şantiye kara defterine işlendi.";
                    return RedirectToAction("Details", "SupplierCurrentAccount", new { id = openedCurrentAccountId.Value });
                }

                TempData["SuccessMessage"] = isPaidUpfront 
                    ? $"Teklif onaylandı! {actualInvoiceAmount:N2} ₺ peşin ödendi olarak cari hesaba ve şantiye kara defterine işlendi."
                    : $"Teklif onaylandı! {actualInvoiceAmount:N2} ₺ vadeli olarak cari hesaba ve şantiye kara defterine işlendi.";
                return RedirectToAction("Details", new { id = quote.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Onaylanırken hata oluştu: " + ex.Message;
                return RedirectToAction("Details", new { id = quoteRequestId });
            }
        }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteSupplier(int quoteId, string companyName, string contactPerson, string phoneNumber, string email, string? workDescription = null)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                var quote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == quoteId);
                if (quote == null || quote.RequesterAgencyId != agencyId) return NotFound();

                var user = await _userManager.GetUserAsync(User);

                var syncResult = await _supplierSyncService.EnsureSupplierSynchronizedAsync(
                    agencyId.Value,
                    user?.Id ?? "",
                    companyName,
                    contactPerson,
                    phoneNumber,
                    email,
                    quote.Title);

                var invite = new B2BQuoteInvite
                {
                    QuoteRequestId = quoteId,
                    NetworkContactId = syncResult.NetworkContact.Id,
                    Status = QuoteInviteStatus.Pending,
                    AccessToken = Guid.NewGuid().ToString("N"),
                    OfferNotes = ""
                };

                _context.B2BQuoteInvites.Add(invite);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(workDescription)) quote.Description = workDescription.Trim();
                if (syncResult.B2bCompany != null && syncResult.B2bCompany.InvitationStatus == GMK360.Core.Entities.B2b.B2bInvitationStatus.Shadow) syncResult.B2bCompany.InvitationStatus = GMK360.Core.Entities.B2b.B2bInvitationStatus.Invited;
                await _context.SaveChangesAsync();
                TempData["NewlyInvitedId"] = invite.Id;
                var smsService = HttpContext.RequestServices.GetService<GMK360.Web.Services.Sms.ISmsService>();
                if (smsService != null && !string.IsNullOrWhiteSpace(phoneNumber)) { try { var portalUrl = $"{Request.Scheme}://{Request.Host}/B2BPartnerPortal/Invite/{invite.AccessToken}"; var smsMsg = $"Merhaba {(string.IsNullOrWhiteSpace(contactPerson) ? companyName : contactPerson)}, {quote.Title} işi için detayları inceleyip fiyat teklifi vermek veya saha keşif randevusu istemek için tıklayın: {portalUrl}"; _ = Task.Run(() => smsService.SmsGonderAsync(phoneNumber, smsMsg)); } catch { } }
                TempData["SuccessMessage"] = $"{companyName} ({contactPerson}) rehbere eklendi ve işin içeriğiyle birlikte özel davet/randevu linki oluşturuldu! Firma linkten sisteme dahil olduğunda Kazan-Kazan referans kazancınız işlenecektir.";
                return RedirectToAction("Details", new { id = quoteId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Davet oluşturulurken hata oluştu: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "");
                return RedirectToAction("Details", new { id = quoteId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteExistingB2bCompany(int quoteId, int b2bCompanyId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var quote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == quoteId);
            if (quote == null || quote.RequesterAgencyId != agencyId) return NotFound();

            var company = await _context.B2bCompanies.Include(c => c.Contacts).FirstOrDefaultAsync(c => c.Id == b2bCompanyId);
            if (company == null) return NotFound();

            var primaryContact = company.Contacts.FirstOrDefault();
            var syncResult = await _supplierSyncService.EnsureSupplierSynchronizedAsync(
                agencyId.Value,
                _userManager.GetUserId(User) ?? "",
                company.Name,
                primaryContact?.FullName ?? company.Name,
                primaryContact?.MobilePhone ?? "",
                primaryContact?.Email ?? "",
                quote.Title);

            var invite = new B2BQuoteInvite
            {
                QuoteRequestId = quoteId,
                NetworkContactId = syncResult.NetworkContact.Id,
                Status = QuoteInviteStatus.Pending,
                AccessToken = Guid.NewGuid().ToString("N"),
                OfferNotes = ""
            };

            _context.B2BQuoteInvites.Add(invite);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{company.Name} başarıyla ihaleye davet edildi!";
            return RedirectToAction(nameof(Details), new { id = quoteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmInspectionAppointment(
            int inviteId, 
            DateTime? confirmedDate, 
            DateTime? appointmentDate, 
            string? appointmentTime, 
            string? managerNotes)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
            if (invite == null || invite.QuoteRequest.RequesterAgencyId != agencyId) return NotFound();

            // Tarih ve saat birleştirme
            DateTime finalDate = confirmedDate ?? DateTime.Today.AddDays(1).AddHours(10);
            if (appointmentDate.HasValue)
            {
                finalDate = appointmentDate.Value.Date;
                if (!string.IsNullOrWhiteSpace(appointmentTime) && TimeSpan.TryParse(appointmentTime, out var ts))
                {
                    finalDate = finalDate.Add(ts);
                }
                else
                {
                    finalDate = finalDate.AddHours(10);
                }
            }

            var notes = string.IsNullOrWhiteSpace(managerNotes) ? "Şantiye şefi usta ile teyit etti." : managerNotes.Trim();

            // Şantiye Kara Defteri'ne (AgendaRecord) KESİN KAYIT aç
            await _supplierSyncService.CreateDiscoveryAgendaRecordAsync(
                inviteId, 
                finalDate, 
                invite.NetworkContact?.ContactPerson, 
                notes);

            // Teklif notunu kesinleşti olarak güncelle
            invite.OfferNotes = $"[RANDEVU KESİNLEŞTİ] Tarih: {finalDate:dd.MM.yyyy HH:mm} | Not: {notes}";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Saha keşif randevusu ({finalDate:dd.MM.yyyy HH:mm}) teyit edildi ve Şantiye Kara Defteri'ne (Ajandaya) işlendi.";
            return RedirectToAction(nameof(Details), new { id = invite.QuoteRequestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BroadcastQuoteToMarketplace(int quoteId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var quote = await _context.B2BQuoteRequests
                .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                .FirstOrDefaultAsync(q => q.Id == quoteId);
            if (quote == null || quote.RequesterAgencyId != agencyId) return NotFound();

            byte contactType = _supplierSyncService.DetermineContactType(quote.Title);
            var query = _context.B2bCompanies
                .Include(c => c.Contacts)
                .AsQueryable();

            if (contactType == 8 || contactType == 4)
            {
                query = query.Where(c => c.IsEngineering);
            }
            else if (contactType == 3)
            {
                query = query.Where(c => c.IsSupplier);
            }
            else
            {
                query = query.Where(c => c.IsSubcontractor);
            }

            var invitedCompanyNames = quote.Invites
                .Where(i => i.NetworkContact != null)
                .Select(i => i.NetworkContact.CompanyName.Trim().ToLower())
                .ToList();

            var eligibleCompanies = await query
                .Where(c => !invitedCompanyNames.Contains(c.Name.Trim().ToLower()))
                .ToListAsync();

            if (!eligibleCompanies.Any())
            {
                TempData["ErrorMessage"] = "Bu sektörde henüz davet edilmemiş açık pazar yeri firması bulunamadı.";
                return RedirectToAction(nameof(Details), new { id = quoteId });
            }

            var currentUserId = _userManager.GetUserId(User) ?? "";
            int count = 0;
            var smsService = HttpContext.RequestServices.GetService<GMK360.Web.Services.Sms.ISmsService>();

            foreach (var company in eligibleCompanies)
            {
                var primaryContact = company.Contacts.FirstOrDefault();
                var syncResult = await _supplierSyncService.EnsureSupplierSynchronizedAsync(
                    agencyId.Value,
                    currentUserId,
                    company.Name,
                    primaryContact?.FullName ?? company.Name,
                    primaryContact?.MobilePhone ?? "",
                    primaryContact?.Email ?? "",
                    quote.Title);

                var invite = new B2BQuoteInvite
                {
                    QuoteRequestId = quoteId,
                    NetworkContactId = syncResult.NetworkContact.Id,
                    Status = QuoteInviteStatus.Pending,
                    AccessToken = Guid.NewGuid().ToString("N"),
                    OfferNotes = "[AÇIK İHALE] Pazar Yeri Genel Duyurusu"
                };

                _context.B2BQuoteInvites.Add(invite);
                count++;

                // SMS Entegrasyonu (Numara varsa sessizce arka planda tetikle)
                if (smsService != null && primaryContact != null && !string.IsNullOrWhiteSpace(primaryContact.MobilePhone))
                {
                    try
                    {
                        var portalUrl = $"{Request.Scheme}://{Request.Host}/B2BPartnerPortal/Invite/{invite.AccessToken}";
                        var msg = $"GMK360 Pazar Yeri: {quote.Title} ihalesi açıldı! Teklif vermek için tıklayın: {portalUrl}";
                        _ = Task.Run(() => smsService.SmsGonderAsync(primaryContact.MobilePhone, msg));
                    }
                    catch { /* SMS hatası ana akışı engellemesin */ }
                }
            }

            quote.Status = "Published";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Açık İhale Pazar Yerine Duyuruldu! İlgili sektördeki {count} firmaya teklif daveti başarıyla iletildi.";
            return RedirectToAction(nameof(Details), new { id = quoteId });
        }
    }
}











