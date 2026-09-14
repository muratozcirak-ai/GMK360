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

        public B2BPurchasingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<int?> GetUserAgencyIdAsync()
        {
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin")) 
                return 1;
                
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            
            var consultant = await _context.Set<GMK360.Core.Entities.AgencyConsultant>().FirstOrDefaultAsync(a => a.UserId == user.Id);
            return consultant?.AgencyId;
        }

        public async Task<IActionResult> Index()
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null && !User.IsInRole("Admin")) return Unauthorized();

                var query = _context.B2BQuoteRequests
                    .Include(q => q.Invites).ThenInclude(i => i.NetworkContact)
                    .AsQueryable();

                if (!User.IsInRole("Admin"))
                {
                    query = query.Where(q => q.RequesterAgencyId == agencyId);
                }

                var quotes = await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
                if (agencyId.HasValue) ViewBag.Catalogs = await _context.MaterialCatalogs.Where(c => c.AgencyId == agencyId.Value).OrderBy(c => c.Name).ToListAsync();
                return View(quotes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Sistem Hatasý: " + ex.Message;
                return View(new System.Collections.Generic.List<B2BQuoteRequest>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuote(string title, string description, DateTime? deadline, int? materialCatalogId, decimal? quantity)
        {
            try 
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();
                
                var user = await _userManager.GetUserAsync(User);

                string finalTitle = title;
                if (string.IsNullOrWhiteSpace(finalTitle) && materialCatalogId.HasValue)
                {
                    var cat = await _context.MaterialCatalogs.FindAsync(materialCatalogId.Value);
                    if (cat != null) finalTitle = $"{quantity} {cat.DefaultUnit} {cat.Name} Alýmý";
                }

                var quote = new B2BQuoteRequest
                {
                    RequesterAgencyId = agencyId.Value,
                    RequesterUserId = user?.Id ?? "",
                    SourceModule = "Direct",
                    SourceReferenceId = 0,
                    Title = finalTitle ?? "Ýsimsiz Alým",
                    Description = description ?? "",
                    Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow
                };

                _context.B2BQuoteRequests.Add(quote);
                await _context.SaveChangesAsync();

                if (materialCatalogId.HasValue && quantity.HasValue)
                {
                    var quoteItem = new B2BQuoteItem
                    {
                        B2BQuoteRequestId = quote.Id,
                        MaterialCatalogId = materialCatalogId.Value,
                        Quantity = quantity.Value,
                        Description = ""
                    };
                    _context.B2BQuoteItems.Add(quoteItem);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Alým Emri / Ýhale baþarýyla oluþturuldu!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Alým kaydý oluþturulurken hata oluþtu: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "");
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
            
            TempData["SuccessMessage"] = "Kalem baþarýyla eklendi.";
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
            
            TempData["SuccessMessage"] = "Kalem listeden çýkarýldý.";
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
                TempData["ErrorMessage"] = "Detaylar yüklenirken hata oluþtu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

                [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAndComplete(int quoteRequestId, int inviteId, int warehouseId, string assignedUserId, int[] inviteItemIds, decimal[] acceptedQuantities)
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
                    WarehouseId = warehouseId,
                    B2BQuoteRequestId = quote.Id,
                    SupplierName = invite.NetworkContact?.CompanyName ?? "Bilinmiyor",
                    DocumentNumber = "B2B-" + quote.Id.ToString(),
                    ReceiptDate = DateTime.UtcNow,
                    Notes = "B2B Sipariþi. Proje/Þantiye teslimatý bekleniyor.",
                    Status = "Draft",
                    CreatedAt = DateTime.UtcNow,
                    AssignedUserId = assignedUserId,
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

                _context.InventoryReceipts.Add(receipt);
                
                if (invite.NetworkContactId > 0 && actualInvoiceAmount > 0)
                {
                    var currentAccount = await _context.SupplierCurrentAccounts
                        .FirstOrDefaultAsync(a => a.AgencyId == agencyId.Value && a.PhonebookContactId == invite.NetworkContactId);
                        
                    if (currentAccount == null)
                    {
                        currentAccount = new GMK360.Core.Entities.Finance.SupplierCurrentAccount
                        {
                            AgencyId = agencyId.Value,
                            PhonebookContactId = invite.NetworkContactId,
                            CurrentBalance = 0
                        };
                        _context.SupplierCurrentAccounts.Add(currentAccount);
                        await _context.SaveChangesAsync(); 
                    }
                    
                    currentAccount.CurrentBalance += actualInvoiceAmount;
                    
                    var transaction = new GMK360.Core.Entities.Finance.SupplierAccountTransaction
                    {
                        SupplierCurrentAccountId = currentAccount.Id,
                        TransactionDate = DateTime.UtcNow,
                        Type = GMK360.Core.Entities.Finance.SupplierTransactionType.PurchaseInvoice,
                        Amount = actualInvoiceAmount,
                        BalanceAfterTransaction = currentAccount.CurrentBalance,
                        Description = $"B2B Kýsmi/Tam Onay - Ýhale #{quote.Id}",
                        DocumentReference = receipt.DocumentNumber,
                        CreatedByUserId = _userManager.GetUserId(User) ?? ""
                    };
                    _context.SupplierAccountTransactions.Add(transaction);
                }
                
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Teklif onaylandý! {actualInvoiceAmount.ToString("N2")} ? cari hesaba iþlendi.";
                return RedirectToAction("Details", new { id = quote.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Onaylanýrken hata oluþtu: " + ex.Message;
                return RedirectToAction("Details", new { id = quoteRequestId });
            }
        }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteSupplier(int quoteId, string companyName, string contactPerson, string phoneNumber, string email)
        {
            try
            {
                var agencyId = await GetUserAgencyIdAsync();
                if (agencyId == null) return Unauthorized();

                var quote = await _context.B2BQuoteRequests.FirstOrDefaultAsync(q => q.Id == quoteId);
                if (quote == null || quote.RequesterAgencyId != agencyId) return NotFound();

                var user = await _userManager.GetUserAsync(User);

                // Create Shadow Contact
                var contact = new B2BNetworkContact
                {
                    OwnerAgencyId = agencyId.Value,
                    AddedByUserId = user?.Id ?? "",
                    CompanyName = companyName ?? "Bilinmeyen Firma",
                    ContactPerson = contactPerson ?? "",
                    PhoneNumber = phoneNumber ?? "",
                    Email = email ?? "",
                    SectorCategory = "Tedarikçi",
                    CreatedAt = DateTime.UtcNow
                };

                _context.B2BNetworkContacts.Add(contact);
                await _context.SaveChangesAsync();

                // Create Invite
                var invite = new B2BQuoteInvite
                {
                    QuoteRequestId = quoteId,
                    NetworkContactId = contact.Id,
                    Status = QuoteInviteStatus.Pending,
                    OfferNotes = ""
                };

                _context.B2BQuoteInvites.Add(invite);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = companyName + " baþarýyla gölge tedarikçi olarak eklendi ve davet oluþturuldu.";
                return RedirectToAction("Details", new { id = quoteId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Davet oluþturulurken hata oluþtu: " + ex.Message + (ex.InnerException != null ? " (" + ex.InnerException.Message + ")" : "");
                return RedirectToAction("Details", new { id = quoteId });
            }
        }
}
}










