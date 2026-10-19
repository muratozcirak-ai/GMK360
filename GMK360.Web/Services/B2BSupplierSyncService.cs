using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;
using GMK360.Core.Entities.B2b;
using GMK360.Core.Entities.B2B;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Finance;

namespace GMK360.Web.Services
{
    public class SupplierSyncResult
    {
        public AgencyPhonebook PhonebookContact { get; set; } = null!;
        public B2bCompany B2bCompany { get; set; } = null!;
        public B2BNetworkContact NetworkContact { get; set; } = null!;
        public B2BNetworkConnection NetworkConnection { get; set; } = null!;
    }

    public class B2BSupplierSyncService
    {
        private readonly ApplicationDbContext _context;

        public B2BSupplierSyncService(ApplicationDbContext context)
        {
            _context = context;
        }

        public byte DetermineContactType(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 2; // Taşeron varsayılan
            var lower = text.ToLowerInvariant();
            
            if (lower.Contains("karot") || lower.Contains("laboratuvar") || lower.Contains("denetim") || 
                lower.Contains("osgb") || lower.Contains("zemin etüd") || lower.Contains("riskli yapı") || 
                lower.Contains("asbest") || lower.Contains("statik"))
            {
                return 8; // Test Laboratuvarı / OSGB
            }
            if (lower.Contains("beton") || lower.Contains("demir") || lower.Contains("tuğla") || 
                lower.Contains("çimento") || lower.Contains("nalbur") || lower.Contains("malzeme") || 
                lower.Contains("seramik") || lower.Contains("kum") || lower.Contains("mıcır"))
            {
                return 3; // Malzeme Tedarikçisi
            }
            if (lower.Contains("mimar") || lower.Contains("mühendis") || lower.Contains("proje") || 
                lower.Contains("harita") || lower.Contains("şantiye şefi"))
            {
                return 4; // Mimar & Mühendislik
            }
            if (lower.Contains("usta") || lower.Contains("boya") || lower.Contains("sıva") || 
                lower.Contains("kalıp") || lower.Contains("demirci") || lower.Contains("tesisat") || 
                lower.Contains("elektrik") || lower.Contains("alçı"))
            {
                return 1; // Usta / Mavi Yaka
            }
            if (lower.Contains("vinç") || lower.Contains("kepçe") || lower.Contains("hafriyat") || 
                lower.Contains("kamyon") || lower.Contains("makine"))
            {
                return 5; // Makine Kiralama
            }

            return 2; // Taşeron Firma
        }

        public async Task<SupplierSyncResult> EnsureSupplierSynchronizedAsync(
            int agencyId, 
            string currentUserId, 
            string companyName, 
            string contactPerson, 
            string phoneNumber, 
            string email, 
            string sectorOrTitle)
        {
            companyName = (companyName ?? "Bilinmeyen Firma").Trim();
            contactPerson = (contactPerson ?? "").Trim();
            phoneNumber = (phoneNumber ?? "").Trim();
            email = (email ?? "").Trim();

            byte contactType = DetermineContactType(sectorOrTitle);

            // 1. AgencyPhonebook Kontrolü & Ekleme
            var phonebook = await _context.AgencyPhonebooks
                .FirstOrDefaultAsync(p => p.AgencyId == agencyId && 
                    ((!string.IsNullOrEmpty(phoneNumber) && p.PhoneNumber == phoneNumber) || p.Name == companyName));

            if (phonebook == null)
            {
                phonebook = new AgencyPhonebook
                {
                    AgencyId = agencyId,
                    Name = companyName,
                    AuthorizedPerson = contactPerson,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    ContactType = contactType,
                    Notes = $"B2B İhale ve Teklif Sistemi üzerinden otomatik eklendi. ({sectorOrTitle})",
                    CreatedAt = DateTime.UtcNow
                };
                _context.AgencyPhonebooks.Add(phonebook);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Mevcutsa ve ContactType genel kaldıysa özelleştir
                if (phonebook.ContactType == 0 || phonebook.ContactType == 2)
                {
                    phonebook.ContactType = contactType;
                }
                if (string.IsNullOrEmpty(phonebook.AuthorizedPerson) && !string.IsNullOrEmpty(contactPerson))
                {
                    phonebook.AuthorizedPerson = contactPerson;
                }
                await _context.SaveChangesAsync();
            }

            // 2. Global B2bCompany Kontrolü & Ekleme (Pazar Yeri Vitrini)
            var b2bCompany = await _context.B2bCompanies
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Name == companyName || 
                    (!string.IsNullOrEmpty(phoneNumber) && c.Contacts.Any(ct => ct.MobilePhone == phoneNumber)));

            if (b2bCompany == null)
            {
                b2bCompany = new B2bCompany
                {
                    Name = companyName,
                    LegalStatus = LegalEntityType.Corporate,
                    IsEngineering = (contactType == 8 || contactType == 4),
                    IsSupplier = (contactType == 3),
                    IsSubcontractor = (contactType == 1 || contactType == 2 || contactType == 5),
                    InvitationStatus = B2bInvitationStatus.Shadow, // Başlangıçta Gölge Kullanıcı
                    AddedByAgencyId = agencyId,
                    AddedByUserId = currentUserId,
                    Rating = 0,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"{companyName} - Şantiye İş Ortağı"
                };

                if (!string.IsNullOrEmpty(contactPerson) || !string.IsNullOrEmpty(phoneNumber))
                {
                    b2bCompany.Contacts.Add(new B2bContact
                    {
                        FullName = string.IsNullOrEmpty(contactPerson) ? companyName : contactPerson,
                        MobilePhone = phoneNumber,
                        Email = email,
                        DepartmentOrRole = "Yetkili",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.B2bCompanies.Add(b2bCompany);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Sektör bayraklarını güncelle
                if (contactType == 8 || contactType == 4) b2bCompany.IsEngineering = true;
                if (contactType == 3) b2bCompany.IsSupplier = true;
                if (contactType == 1 || contactType == 2 || contactType == 5) b2bCompany.IsSubcontractor = true;
                await _context.SaveChangesAsync();
            }

            // 3. B2BNetworkConnection (Bu İnşaat Firması ile Bağlantısı)
            var connection = await _context.B2BNetworkConnections
                .FirstOrDefaultAsync(nc => nc.AgencyId == agencyId && nc.B2bCompanyId == b2bCompany.Id);

            if (connection == null)
            {
                connection = new B2BNetworkConnection
                {
                    AgencyId = agencyId,
                    B2bCompanyId = b2bCompany.Id,
                    IsActive = true,
                    ConnectionNotes = $"İhale daveti ile portföye bağlandı. ({sectorOrTitle})"
                };
                _context.B2BNetworkConnections.Add(connection);
                await _context.SaveChangesAsync();
            }

            // 4. B2BNetworkContact (İhale teklif eşleşmesi için)
            var networkContact = await _context.B2BNetworkContacts
                .FirstOrDefaultAsync(c => c.OwnerAgencyId == agencyId && 
                    ((!string.IsNullOrEmpty(phoneNumber) && c.PhoneNumber == phoneNumber) || c.CompanyName == companyName));

            if (networkContact == null)
            {
                networkContact = new B2BNetworkContact
                {
                    OwnerAgencyId = agencyId,
                    AddedByUserId = currentUserId ?? "",
                    CompanyName = companyName,
                    ContactPerson = contactPerson,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    SectorCategory = sectorOrTitle ?? "Tedarikçi",
                    CreatedAt = DateTime.UtcNow
                };
                _context.B2BNetworkContacts.Add(networkContact);
                await _context.SaveChangesAsync();
            }

            return new SupplierSyncResult
            {
                PhonebookContact = phonebook,
                B2bCompany = b2bCompany,
                NetworkContact = networkContact,
                NetworkConnection = connection
            };
        }

        public async Task AutoSyncExistingNetworkContactsAsync(int agencyId, string currentUserId)
        {
            var unsyncedContacts = await _context.B2BNetworkContacts
                .Where(c => c.OwnerAgencyId == agencyId)
                .ToListAsync();

            foreach (var c in unsyncedContacts)
            {
                await EnsureSupplierSynchronizedAsync(
                    agencyId,
                    currentUserId,
                    c.CompanyName,
                    c.ContactPerson,
                    c.PhoneNumber,
                    c.Email,
                    c.SectorCategory);
            }
        }

        public async Task<SupplierCurrentAccount> OpenOrUpdateCurrentAccountAsync(
            int agencyId,
            int phonebookContactId,
            int? projectId,
            decimal amount,
            string description,
            string documentReference,
            string currentUserId,
            bool isPaidUpfront = false,
            DateTime? dueDate = null)
        {
            var account = await _context.SupplierCurrentAccounts
                .Include(a => a.Transactions)
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.PhonebookContactId == phonebookContactId);

            if (account == null)
            {
                account = new SupplierCurrentAccount
                {
                    AgencyId = agencyId,
                    PhonebookContactId = phonebookContactId,
                    CurrentBalance = 0
                };
                _context.SupplierCurrentAccounts.Add(account);
                await _context.SaveChangesAsync();
            }

            // 1. Borç Tahakkuku (Fatura / Sipariş Kabulü)
            account.CurrentBalance += amount;

            var transaction = new SupplierAccountTransaction
            {
                SupplierCurrentAccountId = account.Id,
                TransactionDate = DateTime.UtcNow,
                Type = SupplierTransactionType.PurchaseInvoice,
                Amount = amount,
                BalanceAfterTransaction = account.CurrentBalance,
                Description = description,
                DocumentReference = documentReference,
                ProjectId = projectId,
                CreatedByUserId = currentUserId ?? ""
            };
            _context.SupplierAccountTransactions.Add(transaction);

            // 2. Ödeme Planı (Peşin mi, Vadeli mi?)
            if (isPaidUpfront)
            {
                // Peşin ödendi -> Hemen kasa/banka çıkışı yap
                account.CurrentBalance -= amount;

                var payment = new SupplierPayment
                {
                    SupplierCurrentAccountId = account.Id,
                    Method = PaymentMethod.BankTransfer,
                    Status = PaymentStatus.Completed,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow,
                    Notes = $"Peşin / Kasa-Banka Çıkışı ile ödendi. ({description})",
                    HandledByUserId = currentUserId ?? ""
                };
                _context.SupplierPayments.Add(payment);

                var paymentTransaction = new SupplierAccountTransaction
                {
                    SupplierCurrentAccountId = account.Id,
                    TransactionDate = DateTime.UtcNow,
                    Type = SupplierTransactionType.PaymentMade,
                    Amount = amount,
                    BalanceAfterTransaction = account.CurrentBalance,
                    Description = $"Ödeme Yapıldı (Peşin) - {description}",
                    DocumentReference = documentReference,
                    ProjectId = projectId,
                    CreatedByUserId = currentUserId ?? ""
                };
                _context.SupplierAccountTransactions.Add(paymentTransaction);
            }
            else
            {
                // Vadeli -> Vade tarihi ile bekleyen ödeme oluştur
                var targetDueDate = dueDate ?? DateTime.UtcNow.AddDays(30);

                var payment = new SupplierPayment
                {
                    SupplierCurrentAccountId = account.Id,
                    Method = PaymentMethod.OpenAccount,
                    Status = PaymentStatus.Pending,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow,
                    DueDate = targetDueDate,
                    Notes = $"Vadeli Alım. Ödeme Vadesi: {targetDueDate:dd.MM.yyyy} ({description})",
                    HandledByUserId = currentUserId ?? ""
                };
                _context.SupplierPayments.Add(payment);
            }

            await _context.SaveChangesAsync();
            return account;
        }

        /// <summary>
        /// Tedarikçi portalından 'Sahada Keşif / Randevu İstiyorum' dendiğinde
        /// Şantiye Kara Defterine (AgendaRecord) anında randevu kaydı açar.
        /// </summary>
        public async Task<AgendaRecord?> CreateDiscoveryAgendaRecordAsync(
            int inviteId, 
            DateTime inspectionDate, 
            string? requestedByPerson,
            string? notes)
        {
            var invite = await _context.B2BQuoteInvites
                .Include(i => i.NetworkContact)
                .Include(i => i.QuoteRequest)
                    .ThenInclude(qr => qr.RequesterAgency)
                .FirstOrDefaultAsync(i => i.Id == inviteId);

            if (invite == null) return null;

            int agencyId = invite.QuoteRequest.RequesterAgencyId;
            int? projectId = null;

            // Proje ID'sini bul
            if (invite.QuoteRequest.SourceModule == "PhaseZeroDocument")
            {
                var doc = await _context.ProjectLegalDocuments
                    .FirstOrDefaultAsync(d => d.Id == invite.QuoteRequest.SourceReferenceId);
                if (doc != null) projectId = doc.ConstructionProjectId;
            }
            else if (invite.QuoteRequest.SourceModule == "ConstructionProject")
            {
                projectId = invite.QuoteRequest.SourceReferenceId;
            }

            // Phonebook ID eşleştirmesi
            int? phonebookId = null;
            if (invite.NetworkContact != null)
            {
                var pb = await _context.AgencyPhonebooks
                    .FirstOrDefaultAsync(p => p.AgencyId == agencyId && 
                        ((!string.IsNullOrEmpty(invite.NetworkContact.PhoneNumber) && p.PhoneNumber == invite.NetworkContact.PhoneNumber) ||
                         p.Name == invite.NetworkContact.CompanyName));
                if (pb != null) phonebookId = pb.Id;
            }

            var companyName = invite.NetworkContact?.CompanyName ?? "Tedarikçi";
            var contactPhone = invite.NetworkContact?.PhoneNumber ?? "";
            var title = $"🔍 [Saha Keşfi] {companyName} - {invite.QuoteRequest.Title}";

            var agendaRecord = new AgendaRecord
            {
                AgencyId = agencyId,
                ProjectId = projectId,
                PhonebookId = phonebookId,
                Title = title,
                EventDate = inspectionDate,
                ImageUrl = string.Empty,
                RecordTypeId = AgendaConstants.TypeTaseronToplantisi,
                StatusId = AgendaConstants.StatusPlanlandi,
                Notes = $"📅 TEDARİKÇİ SAHA KEŞİF RANDEVUSU:\n" +
                        $"Firma: {companyName}\n" +
                        $"Yetkili: {requestedByPerson ?? invite.NetworkContact?.ContactPerson ?? "Belirtilmedi"} (Tel: {contactPhone})\n" +
                        $"Talep Edilen İhale: {invite.QuoteRequest.Title}\n" +
                        $"Durum: Tedarikçi teklif vermeden önce sahada yerinde inceleme / keşif yapacaktır.\n" +
                        $"Randevu Notu: {(string.IsNullOrWhiteSpace(notes) ? "Özel not iletilmedi." : notes)}",
                CreatedAt = DateTime.UtcNow
            };

            _context.AgendaRecords.Add(agendaRecord);

            // Invite kaydını da güncelle
            invite.OfferNotes = $"📅 KEŞİF RANDEVUSU: {inspectionDate:dd.MM.yyyy HH:mm} - {(string.IsNullOrWhiteSpace(notes) ? "Saha tespiti yapılacak" : notes)}";
            invite.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return agendaRecord;
        }

        /// <summary>
        /// İhale veya Faz 0 teklifi kabul edildiğinde
        /// Şantiye Kara Defterine (AgendaRecord) Teslimat & İmalat Takibi görevi açar.
        /// </summary>
        public async Task<AgendaRecord> CreateDeliveryAgendaRecordAsync(
            int agencyId,
            int? projectId,
            int? phonebookId,
            string supplierName,
            string workTitle,
            decimal amount,
            string paymentTerms,
            DateTime targetDate)
        {
            var agendaRecord = new AgendaRecord
            {
                AgencyId = agencyId,
                ProjectId = projectId,
                PhonebookId = phonebookId,
                Title = $"🚚 [Teslimat/İmalat] {supplierName} - {workTitle}",
                EventDate = targetDate,
                ImageUrl = string.Empty,
                RecordTypeId = AgendaConstants.TypeSantiyeDenetimi,
                StatusId = AgendaConstants.StatusPlanlandi,
                Notes = $"🎯 ONAYLANAN SİPARİŞ / TEKLİF TAKİBİ:\n" +
                        $"Tedarikçi: {supplierName}\n" +
                        $"İş / Kalem: {workTitle}\n" +
                        $"Tutar: {amount:N2} TL ({paymentTerms})\n" +
                        $"Takip Görevi: Şantiye şefi sahada malzeme/rapor teslimatını ve montajı denetleyecektir. " +
                        $"Teslimat eksiksiz onaylandığında cari hesap ödemesi serbest bırakılacaktır.",
                CreatedAt = DateTime.UtcNow
            };

            _context.AgendaRecords.Add(agendaRecord);
            await _context.SaveChangesAsync();
            return agendaRecord;
        }
    }
}
