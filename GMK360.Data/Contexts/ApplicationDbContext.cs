using GMK360.Core.Entities;
using GMK360.Core.Entities.Auditing;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GMK360.Data.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        // Konum (Lokasyon) Tabloları
        // Financial Settings
        public DbSet<PaymentSetting> PaymentSettings { get; set; }

        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Neighborhood> Neighborhoods { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<Complex> Complexes { get; set; }
        public DbSet<ComplexBlock> ComplexBlocks { get; set; }
        public DbSet<NeighborhoodPOI> NeighborhoodPOIs { get; set; }
        public DbSet<LocalProfessional> LocalProfessionals { get; set; }
        public DbSet<NeighboringArea> NeighboringAreas { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }



        public DbSet<AddressLocation> AddressLocations { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<DefinitionCategory> DefinitionCategories { get; set; }
        public DbSet<DefinitionValue> DefinitionValues { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<PropertyFeature> PropertyFeatures { get; set; }
        public DbSet<ComplexFeature> ComplexFeatures { get; set; }
        public DbSet<PropertyPriceHistory> PropertyPriceHistories { get; set; }
        public DbSet<PropertyViewHistory> PropertyViewHistories { get; set; }
        
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<AgencyWebBlock> AgencyWebBlocks { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyTranslation> PropertyTranslations { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<PropertyReservation> PropertyReservations { get; set; }
        public DbSet<GuestCheckInRecord> GuestCheckInRecords { get; set; }
        public DbSet<Partner> Partners { get; set; }
        
        // Emlakçı Profil (EİDS & Yetki Belgesi)
        public DbSet<AgentProfile> AgentProfiles { get; set; }
        
        // CRM / B2B Lead Tablosu
        public DbSet<B2BLead> B2BLeads { get; set; }

        // Paket ve Abonelik (Aidat) Sistemi
        public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
        public DbSet<AgentSubscription> AgentSubscriptions { get; set; }

        // Cüzdan & Vitrin Modülü
        public DbSet<UserWalletTransaction> UserWalletTransactions { get; set; }
        public DbSet<GMK360.Core.Entities.Marketing.ShowcasePackage> ShowcasePackages { get; set; }

        // Dinamik Kampanyalar (Kayıt ve Giriş Ekranı)
        public DbSet<GMK360.Core.Entities.Marketing.AuthScreenBanner> AuthScreenBanners { get; set; }

        // Sistem Hata/Uyarı Bildirimleri (Admin Onayı Gereken Durumlar)
        public DbSet<SystemIssueTicket> SystemIssueTickets { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        
        // Yeni Mimaride Eklenenler
        public DbSet<AgencyConsultant> AgencyConsultants { get; set; }
        
        // --- KAMPANYA VE İSKONTO MOTORU (FAZ 23) ---
        public DbSet<GMK360.Core.Entities.Marketing.DiscountPolicy> DiscountPolicies { get; set; }
        public DbSet<GMK360.Core.Entities.Marketing.UserDiscount> UserDiscounts { get; set; }
        
        public DbSet<WalletCredit> WalletCredits { get; set; }

        // Genel Sistem Logları (Merge, Kritik Silmeler vb.)
        public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<SeoSetting> SeoSettings { get; set; }

        public DbSet<UserComparisonNote> UserComparisonNotes { get; set; }
        public DbSet<PropertyFinancialRecord> PropertyFinancialRecords { get; set; }
        public DbSet<TenancyContract> TenancyContracts { get; set; }
        public DbSet<ApiCredential> ApiCredentials { get; set; }
        
        // Yeni Eklenen Dinamik Vergi ve Ödeme Kuralları Modülü
        public DbSet<FinancialObligationType> FinancialObligationTypes { get; set; }
        public DbSet<PropertyFinancialSchedule> PropertyFinancialSchedules { get; set; }
        public DbSet<IncomeTaxDeclaration> IncomeTaxDeclarations { get; set; }
        public DbSet<TaxParameter> TaxParameters { get; set; }
        public DbSet<PropertyExpense> PropertyExpenses { get; set; }
        public DbSet<BuildingExpenseShare> BuildingExpenseShares { get; set; }
        public DbSet<InvitationToken> InvitationTokens { get; set; }        
        // Arama Motoru ve İzolasyon Modeli
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<SavedSearch> SavedSearches { get; set; }
        public DbSet<ScrapedProperty> ScrapedProperties { get; set; }
        public DbSet<MarketAnalytics> MarketAnalytics { get; set; }
        public DbSet<ContactLog> ContactLogs { get; set; }
        public DbSet<ConsultantRating> ConsultantRatings { get; set; }

        // Usta/Esnaf ve Cüzdan
        public DbSet<ServiceProvider> ServiceProviders { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ServiceProviderService> ServiceProviderServices { get; set; }
        public DbSet<ServiceProviderPortfolio> ServiceProviderPortfolios { get; set; }
        public DbSet<ServiceProviderArea> ServiceProviderAreas { get; set; }
        public DbSet<ServiceAppointment> ServiceAppointments { get; set; }
        public DbSet<ServiceProviderRating> ServiceProviderRatings { get; set; }
        public DbSet<WalletWithdrawalRequest> WalletWithdrawalRequests { get; set; }
        public DbSet<ServiceProviderDocument> ServiceProviderDocuments { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        // Fatura ve Muhasebe
        public DbSet<UserBillingInfo> UserBillingInfos { get; set; }
        public DbSet<InvoiceRecord> InvoiceRecords { get; set; }
        public DbSet<KesilenFatura> KesilenFaturalar { get; set; }

        // KBS Emniyet Entegrasyonu
        public DbSet<KbsFacilitySettings> KbsFacilitySettings { get; set; }

        // B2B Mini CRM ve Ajanda
        public DbSet<CrmContact> CrmContacts { get; set; }
        public DbSet<CrmDemand> CrmDemands { get; set; }
        public DbSet<CrmAppointment> CrmAppointments { get; set; }
        public DbSet<CrmRentalTracking> CrmRentalTrackings { get; set; }

        // B2B Nalbur ve Tedarikçi Teklif Sistemi
        public DbSet<B2bSupplier> B2bSuppliers { get; set; }
        public DbSet<MaterialList> MaterialLists { get; set; }
        public DbSet<MaterialListItem> MaterialListItems { get; set; }
        public DbSet<MaterialListOffer> MaterialListOffers { get; set; }

        // CMS ve Bildirim Sistemi
        public DbSet<Article> Articles { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<EidsValidationLog> EidsValidationLogs { get; set; }
        
        // Mülk Muhasebesi ve Gider Takibi Modülü
        public DbSet<LiabilityType> LiabilityTypes { get; set; }
        public DbSet<PropertyPayment> PropertyPayments { get; set; }
        public DbSet<DigitalDocument> DigitalDocuments { get; set; }
        public DbSet<UtilityCompany> UtilityCompanies { get; set; }
        public DbSet<PropertyLiability> PropertyLiabilities { get; set; }

        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        
        // Tadilat ve İhale Modülü (Renovation Marketplace)
        public DbSet<RenovationRequest> RenovationRequests { get; set; }
        public DbSet<RenovationOffer> RenovationOffers { get; set; }

        // Dijital Sözleşmeler Merkezi
        public DbSet<DigitalContract> DigitalContracts { get; set; }

        // Bina ve Site Yönetim Modülü
        public DbSet<Building> Buildings { get; set; }
        public DbSet<BuildingUnit> BuildingUnits { get; set; }
        public DbSet<UnitSpace> UnitSpaces { get; set; }
 
        public DbSet<SpaceMeasurement> SpaceMeasurements { get; set; }
        public DbSet<SpaceFixture> SpaceFixtures { get; set; }
        public DbSet<MaterialOption> MaterialOptions { get; set; }
       public DbSet<UnitTemplate> UnitTemplates { get; set; }
        public DbSet<UnitTemplateSpace> UnitTemplateSpaces { get; set; }
        public DbSet<ProjectAmenity> ProjectAmenities { get; set; }
        public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
        public DbSet<BuildingExpense> BuildingExpenses { get; set; }
        public DbSet<BuildingIncome> BuildingIncomes { get; set; }
        public DbSet<BuildingContract> BuildingContracts { get; set; }
        public DbSet<BolgeEkspertizHafizasi> BolgeEkspertizHafizalari { get; set; }
        public DbSet<UnitDebt> UnitDebts { get; set; }
        public DbSet<BuildingAnnouncement> BuildingAnnouncements { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<DocumentArchive> DocumentArchives { get; set; }

        // Kurumsal Yönetim Şirketi ve Siteler
        public DbSet<ManagementCompany> ManagementCompanies { get; set; }
        public DbSet<HousingComplex> HousingComplexes { get; set; }

        // İnşaat ve Şantiye Yönetimi (Kentsel Dönüşüm ERP)
        public DbSet<ConstructionProject> ConstructionProjects { get; set; }
        public DbSet<ConstructionTimesheet> ConstructionTimesheets { get; set; }
        public DbSet<ProjectPhase> ProjectPhases { get; set; }
        public DbSet<ProjectMaterialCatalog> ProjectMaterialCatalogs { get; set; }
        public DbSet<UnitMaterialSelection> UnitMaterialSelections { get; set; }
        public DbSet<ConstructionTask> ConstructionTasks { get; set; }
        public DbSet<ConstructionTaskInvite> ConstructionTaskInvites { get; set; }
        public DbSet<TaskProgressLog> TaskProgressLogs { get; set; }

        // B2B ve Pazaryeri (Marketplace)
        public DbSet<SupplierTradesmanRelation> SupplierTradesmanRelations { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<MaterialPriceInquiry> MaterialPriceInquiries { get; set; }
        public DbSet<UniversalSurvey> UniversalSurveys { get; set; }
        public DbSet<SupplierCampaign> SupplierCampaigns { get; set; }

        // Evrensel Finans ve Ödeme Altyapısı
        public DbSet<FinancialAccount> FinancialAccounts { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PlatformCommissionRate> PlatformCommissionRates { get; set; }

        // Günlük Kiralama
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<GuestIdentity> GuestIdentities { get; set; }
        public DbSet<FiatTransaction> FiatTransactions { get; set; }
        public DbSet<ReferenceLog> ReferenceLogs { get; set; }

        // Davet Sistemi
        
        // --- PHASE 7 BİNA YÖNETİM ROLLERİ ---
        public DbSet<BuildingManager> BuildingManagers { get; set; }
        
        // --- PHASE 8 GENERIC MEETINGS & SURVEYS ---
        public DbSet<SystemMeeting> SystemMeetings { get; set; }
        public DbSet<SystemMeetingDecision> SystemMeetingDecisions { get; set; }
        public DbSet<MeetingSurvey> MeetingSurveys { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }
        public DbSet<SurveyVote> SurveyVotes { get; set; }
        // --- PHASE 4 & PHASE 7 (YASAL YÖNETİM & DİJİTAL KARAR DEFTERİ) ---
        public DbSet<LegalAgreement> LegalAgreements { get; set; }
        public DbSet<UserAgreementAcceptance> UserAgreementAcceptances { get; set; }
        public DbSet<SmsTemplate> SmsTemplates { get; set; }
        public DbSet<UserAgenda> UserAgendas { get; set; }
        public DbSet<ManagementDecision> ManagementDecisions { get; set; }
        public DbSet<GlobalObligationRule> GlobalObligationRules { get; set; }

        // PHASE 9: Finance, Escrow, and Tax
        public DbSet<GlobalFinanceSettings> GlobalFinanceSettings { get; set; } = null!;
        public DbSet<UserFinancialProfile> UserFinancialProfiles { get; set; } = null!;
        public DbSet<SystemFund> SystemFunds { get; set; } = null!;
        public DbSet<SystemFundTransaction> SystemFundTransactions { get; set; } = null!;
        public DbSet<EscrowTransaction> EscrowTransactions { get; set; } = null!;
        public DbSet<CorporateProfile> CorporateProfiles { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        
        // Bildirim & Personel Muhasebesi (Phase 10 & 11)
        public DbSet<SystemNotificationLog> SystemNotificationLogs { get; set; }
        public DbSet<ManagementMember> ManagementMembers { get; set; }
        public DbSet<StaffPayroll> StaffPayrolls { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<DocumentRegistry> DocumentRegistries { get; set; }
        public DbSet<PropertyUser> PropertyUsers { get; set; }

        // DMS (Digital Management System)
        public DbSet<DmsCabinet> DmsCabinets { get; set; }
        public DbSet<DmsShelf> DmsShelves { get; set; }
        public DbSet<DmsFolder> DmsFolders { get; set; }
        public DbSet<DmsDocument> DmsDocuments { get; set; }

        // Akıllı Hizmet ve Usta Modülü
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceOffer> ServiceOffers { get; set; }
        public DbSet<ServiceProviderSubscription> ServiceProviderSubscriptions { get; set; }

        
        private void SetGlobalQueryFilter<T>(ModelBuilder builder) where T : BaseEntity
        {
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
        {
            HandleSoftDeleteAndAudit();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDeleteAndAudit();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void HandleSoftDeleteAndAudit()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)).ToList();
            
            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                    
                    // Simple Audit for Soft Delete
                    AuditLogs.Add(new AuditLog {
                        ActionType = "SOFT_DELETE",
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entity.Id.ToString(),
                        Timestamp = DateTime.UtcNow,
                        OldValues = "{}",
                        NewValues = "{\"IsDeleted\": true}",
                        AffectedColumns = "[\"IsDeleted\"]",
                        UserId = "SYSTEM"
                    });
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GMK360.Core.Entities.BuildingUnit>()
                .HasOne(u => u.ParentUnit)
                .WithMany(u => u.Attachments)
                .HasForeignKey(u => u.ParentUnitId)
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);


            // Apply Global Query Filter for Soft Delete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(GMK360.Core.Entities.BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ApplicationDbContext).GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var genericMethod = method.MakeGenericMethod(entityType.ClrType);
                    genericMethod.Invoke(this, new object[] { builder });
                }
            }

            builder.Entity<GMK360.Core.Entities.Construction.ConstructionTimesheet>()
                .HasOne(t => t.Worker)
                .WithMany()
                .HasForeignKey(t => t.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GMK360.Core.Entities.Construction.ConstructionTimesheet>()
                .HasOne(t => t.RecordedBy)
                .WithMany()
                .HasForeignKey(t => t.RecordedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Usta - Hizmet Çoka Çok İlişkisi
            builder.Entity<ServiceProviderService>()
                .HasKey(sps => new { sps.ServiceProviderId, sps.ServiceCategoryId });

            builder.Entity<ServiceProviderService>()
                .HasOne(sps => sps.ServiceProvider)
                .WithMany(sp => sp.Services)
                .HasForeignKey(sps => sps.ServiceProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceProviderService>()
                .HasOne(sps => sps.ServiceCategory)
                .WithMany(sc => sc.ProviderServices)
                .HasForeignKey(sps => sps.ServiceCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Usta - Bölge Çoka Çok İlişkisi
            builder.Entity<ServiceProviderArea>()
                .HasKey(spa => new { spa.ServiceProviderId, spa.CityId, spa.DistrictId });

            builder.Entity<ServiceProviderArea>()
                .HasOne(spa => spa.ServiceProvider)
                .WithMany(sp => sp.Areas)
                .HasForeignKey(spa => spa.ServiceProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ServiceProviderArea>()
                .HasOne(spa => spa.District)
                .WithMany()
                .HasForeignKey(spa => spa.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceProviderArea>()
                .HasOne(spa => spa.City)
                .WithMany()
                .HasForeignKey(spa => spa.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Randevu (ServiceAppointment)
            builder.Entity<ServiceAppointment>()
                .HasOne(sa => sa.Customer)
                .WithMany()
                .HasForeignKey(sa => sa.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceAppointment>()
                .HasOne(sa => sa.ServiceProvider)
                .WithMany(sp => sp.Appointments)
                .HasForeignKey(sa => sa.ServiceProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many ilişkisi
            builder.Entity<AgencyConsultant>()
                .HasKey(ac => new { ac.AgencyId, ac.UserId });

            builder.Entity<AgencyConsultant>()
                .HasOne(ac => ac.Agency)
                .WithMany(a => a.AgencyConsultants)
                .HasForeignKey(ac => ac.AgencyId);

            builder.Entity<AgencyConsultant>()
                .HasOne(ac => ac.User)
                .WithMany(u => u.AgencyConsultants)
                .HasForeignKey(ac => ac.UserId);

            // CRM Rental Tracking Cascade Delete Çözümü
            builder.Entity<CrmRentalTracking>()
                .HasOne(r => r.Tenant)
                .WithMany(c => c.TenantContracts)
                .HasForeignKey(r => r.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CrmRentalTracking>()
                .HasOne(r => r.Landlord)
                .WithMany(c => c.LandlordContracts)
                .HasForeignKey(r => r.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cascade delete sorunu çözümü (Multiple Cascade Paths)
            builder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(co => co.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complex>()
                .HasOne(c => c.City)
                .WithMany()
                .HasForeignKey(c => c.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complex>()
                .HasOne(c => c.District)
                .WithMany()
                .HasForeignKey(c => c.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complex>()
                .HasOne(c => c.Neighborhood)
                .WithMany()
                .HasForeignKey(c => c.NeighborhoodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Complex>()
                .HasOne(c => c.Street)
                .WithMany()
                .HasForeignKey(c => c.StreetId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Property>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict); // İlanı giren kullanıcı silinirse ilanlar silinmesin (Restrict)

            builder.Entity<Property>()
                .HasOne(p => p.OwnerUser)
                .WithMany() // ApplicationUser modeline ayrı bir koleksiyon eklemeye gerek yok şimdilik
                .HasForeignKey(p => p.OwnerUserId)
                .OnDelete(DeleteBehavior.SetNull); // Mülk sahibi silinirse, ilandaki OwnerUserId null olsun

            builder.Entity<Property>()
                .HasOne(p => p.Agency)
                .WithMany()
                .HasForeignKey(p => p.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // İlişkileri belirleme
            builder.Entity<DefinitionValue>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Values)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Kategori Hiyerarşisi (Soru Grubu -> Sorular)
            builder.Entity<DefinitionCategory>()
                .HasOne(dc => dc.ParentCategory)
                .WithMany(pc => pc.SubCategories)
                .HasForeignKey(dc => dc.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // B2B Tedarikçi (Nalbur) İlişkileri - Cascade Delete Hatalarını Önlemek İçin
            builder.Entity<B2bSupplier>()
                .HasOne(s => s.City)
                .WithMany()
                .HasForeignKey(s => s.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<B2bSupplier>()
                .HasOne(s => s.District)
                .WithMany()
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialListOffer>()
                .HasOne(o => o.Supplier)
                .WithMany()
                .HasForeignKey(o => o.B2bSupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // Komşu Mahalle Self-Referencing İlişkisi (EF Core Configuration)
            builder.Entity<NeighboringArea>()
                .HasOne(na => na.BaseNeighborhood)
                .WithMany()
                .HasForeignKey(na => na.BaseNeighborhoodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<NeighboringArea>()
                .HasOne(na => na.NeighborNeighborhood)
                .WithMany()
                .HasForeignKey(na => na.NeighborNeighborhoodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cascade delete ayarları
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Offer Configuration
            builder.Entity<Offer>()
                .HasOne(o => o.Buyer)
                .WithMany()
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Offer>()
                .HasOne(o => o.Seller)
                .WithMany()
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyFeature>()
                .HasOne(pf => pf.Property)
                .WithMany(p => p.Features)
                .HasForeignKey(pf => pf.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PropertyFeature>()
                .HasOne(pf => pf.DefinitionValue)
                .WithMany()
                .HasForeignKey(pf => pf.DefinitionValueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ComplexFeature>()
                .HasOne(cf => cf.Complex)
                .WithMany(c => c.Features)
                .HasForeignKey(cf => cf.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ComplexFeature>()
                .HasOne(cf => cf.DefinitionValue)
                .WithMany()
                .HasForeignKey(cf => cf.DefinitionValueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Property>()
                .HasOne(p => p.Status)
                .WithMany()
                .HasForeignKey(p => p.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Property>()
                .HasOne(p => p.Type)
                .WithMany()
                .HasForeignKey(p => p.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PropertyReservation>()
                .Property(pr => pr.CommissionAmount)
                .HasPrecision(18, 2);

            builder.Entity<PropertyReservation>()
                .Property(pr => pr.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<PropertyReservation>()
                .Property(pr => pr.PaidAmount)
                .HasPrecision(18, 2);

            builder.Entity<PropertyReservation>()
                .HasOne(pr => pr.Partner)
                .WithMany(p => p.Reservations)
                .HasForeignKey(pr => pr.PartnerId)
                .OnDelete(DeleteBehavior.SetNull);



            // SEED DATA
            builder.Entity<Country>().HasData(
                new Country { Id = 1, Name = "Türkiye", Code = "TR", CreatedAt = new System.DateTime(2024, 1, 1) }
            );

            builder.Entity<City>().HasData(
                new City { Id = 34, CountryId = 1, Name = "İstanbul", PlateCode = "34", CreatedAt = new System.DateTime(2024, 1, 1) },
                new City { Id = 6, CountryId = 1, Name = "Ankara", PlateCode = "06", CreatedAt = new System.DateTime(2024, 1, 1) },
                new City { Id = 35, CountryId = 1, Name = "İzmir", PlateCode = "35", CreatedAt = new System.DateTime(2024, 1, 1) }
            );

            builder.Entity<District>().HasData(
                new District { Id = 1, CityId = 34, Name = "Kadıköy", CreatedAt = new System.DateTime(2024, 1, 1) },
                new District { Id = 2, CityId = 34, Name = "Beşiktaş", CreatedAt = new System.DateTime(2024, 1, 1) },
                new District { Id = 3, CityId = 34, Name = "Şişli", CreatedAt = new System.DateTime(2024, 1, 1) },
                new District { Id = 4, CityId = 6, Name = "Çankaya", CreatedAt = new System.DateTime(2024, 1, 1) },
                new District { Id = 5, CityId = 6, Name = "Keçiören", CreatedAt = new System.DateTime(2024, 1, 1) },
                new District { Id = 6, CityId = 35, Name = "Karşıyaka", CreatedAt = new System.DateTime(2024, 1, 1) },
                new District { Id = 7, CityId = 35, Name = "Bornova", CreatedAt = new System.DateTime(2024, 1, 1) }
            );

            builder.Entity<Neighborhood>().HasData(
                new Neighborhood { Id = 1, DistrictId = 1, Name = "Acıbadem", CreatedAt = new System.DateTime(2024, 1, 1) },
                new Neighborhood { Id = 2, DistrictId = 1, Name = "Bostancı", CreatedAt = new System.DateTime(2024, 1, 1) },
                new Neighborhood { Id = 3, DistrictId = 4, Name = "Bahçelievler", CreatedAt = new System.DateTime(2024, 1, 1) }
            );

            builder.Entity<Street>().HasData(
                new Street { Id = 1, NeighborhoodId = 1, Name = "Gül Sokak", CreatedAt = new System.DateTime(2024, 1, 1) },
                new Street { Id = 2, NeighborhoodId = 1, Name = "Lale Sokak", CreatedAt = new System.DateTime(2024, 1, 1) }
            );

            // SEED AUTH SCREEN BANNERS (Kampanyalar)
            builder.Entity<GMK360.Core.Entities.Marketing.AuthScreenBanner>().HasData(
                new GMK360.Core.Entities.Marketing.AuthScreenBanner
                {
                    Id = 1,
                    Title = "Sektörün Zirvesine Çıkın.",
                    Subtitle = "Yeni nesil emlak platformuna katılarak ilanlarınızı milyonlara ulaştırın veya hayalinizdeki evi bulun.",
                    ImageUrl = "/images/auth_bg.jpg",
                    ActionText = "Hemen İlan Ver",
                    ActionUrl = "/Property/Create",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                },
                new GMK360.Core.Entities.Marketing.AuthScreenBanner
                {
                    Id = 2,
                    Title = "İlk 3 Üyeye Özel Fırsat!",
                    Subtitle = "Bulunduğunuz ildeki ilk 3 kurumsal üye arasına girin, 1 yıllık Premium Vitrin paketini anında kapın.",
                    ImageUrl = "https://images.unsplash.com/photo-1560518883-ce09059eeffa?ixlib=rb-4.0.3&auto=format&fit=crop&w=1000&q=80",
                    ActionText = "Kampanyaya Katıl",
                    ActionUrl = "/Account/Register?referralCode=ILK3UYE",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                }
            );

            // SEED FINANCIAL OBLIGATION TYPES (Vergi ve Yasal Ödemeler)
            builder.Entity<FinancialObligationType>().HasData(
                new FinancialObligationType
                {
                    Id = 1,
                    Name = "Emlak Vergisi",
                    TargetPropertyType = TargetPropertyType.All,
                    ResponsibleRole = ResponsibleRole.Owner,
                    PaymentFrequency = PaymentFrequency.Biannual,
                    FirstInstallmentMonth = 5,  // Mayıs
                    SecondInstallmentMonth = 11, // Kasım
                    IsActive = true,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                },
                new FinancialObligationType
                {
                    Id = 2,
                    Name = "Kira Gelir Vergisi (GMSİ)",
                    TargetPropertyType = TargetPropertyType.All,
                    ResponsibleRole = ResponsibleRole.Owner,
                    PaymentFrequency = PaymentFrequency.Biannual,
                    FirstInstallmentMonth = 3,  // Mart
                    SecondInstallmentMonth = 7, // Temmuz
                    IsActive = true,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                },
                new FinancialObligationType
                {
                    Id = 3,
                    Name = "Çevre Temizlik Vergisi (ÇTV)",
                    TargetPropertyType = TargetPropertyType.All,
                    ResponsibleRole = ResponsibleRole.Tenant,
                    PaymentFrequency = PaymentFrequency.Biannual,
                    FirstInstallmentMonth = 5,
                    SecondInstallmentMonth = 11,
                    IsActive = true,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                },
                new FinancialObligationType
                {
                    Id = 4,
                    Name = "İlan ve Reklam Vergisi (Tabela)",
                    TargetPropertyType = TargetPropertyType.OnlyCommercial, // Sadece İşyeri
                    ResponsibleRole = ResponsibleRole.Tenant,
                    PaymentFrequency = PaymentFrequency.Biannual,
                    FirstInstallmentMonth = 5,  // Mayıs
                    SecondInstallmentMonth = 11, // Kasım
                    IsActive = true,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                },
                new FinancialObligationType
                {
                    Id = 5,
                    Name = "Kira Stopajı (Muhtasar Beyanname)",
                    TargetPropertyType = TargetPropertyType.OnlyCommercial, // Sadece İşyeri
                    ResponsibleRole = ResponsibleRole.Tenant,
                    PaymentFrequency = PaymentFrequency.Monthly, // Genelde aylık veya 3 aylık beyan edilir, aylık seçiyoruz
                    FirstInstallmentMonth = null,
                    SecondInstallmentMonth = null,
                    IsActive = true,
                    CreatedAt = new System.DateTime(2024, 1, 1)
                }
            );
            builder.Entity<ConsultantRating>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ConsultantRating>()
                .HasOne(cr => cr.Consultant)
                .WithMany()
                .HasForeignKey(cr => cr.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ContactLog>()
                .HasOne(cl => cl.User)
                .WithMany()
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ContactLog>()
                .HasOne(cl => cl.Consultant)
                .WithMany()
                .HasForeignKey(cl => cl.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<ContactLog>()
                .HasOne(cl => cl.ServiceProvider)
                .WithMany()
                .HasForeignKey(cl => cl.ServiceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceProvider>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<ServiceProviderRating>()
                .HasOne(spr => spr.ServiceProvider)
                .WithMany(sp => sp.Ratings)
                .HasForeignKey(spr => spr.ServiceProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ServiceProviderRating>()
                .HasOne(spr => spr.User)
                .WithMany()
                .HasForeignKey(spr => spr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WalletWithdrawalRequest>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<UserBillingInfo>()
                .HasOne(ubi => ubi.User)
                .WithMany()
                .HasForeignKey(ubi => ubi.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InvoiceRecord>()
                .HasOne(ir => ir.User)
                .WithMany()
                .HasForeignKey(ir => ir.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EidsValidationLog
            builder.Entity<EidsValidationLog>()
                .HasOne(e => e.Property)
                .WithMany(p => p.EidsLogs)
                .HasForeignKey(e => e.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EidsValidationLog>()
                .HasOne(e => e.Consultant)
                .WithMany(u => u.EidsLogs)
                .HasForeignKey(e => e.ConsultantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mülk Muhasebesi Konfigürasyonları (Masaüstü/Özel İsimlendirmeler)
            builder.Entity<LiabilityType>().ToTable("tbl_gmk_YukumlulukTipleri");
            builder.Entity<LiabilityType>().Property(l => l.Name).HasColumnName("TipAdi").HasMaxLength(100);
            builder.Entity<LiabilityType>().Property(l => l.Category).HasColumnName("Kategori").HasMaxLength(50);
            builder.Entity<LiabilityType>().Property(l => l.IsSystemType).HasColumnName("SistemTipiMi");

            builder.Entity<PropertyPayment>().ToTable("tbl_gmk_Odemeler");
            builder.Entity<PropertyPayment>().Property(p => p.PropertyId).HasColumnName("MulkId");
            builder.Entity<PropertyPayment>().Property(p => p.LiabilityTypeId).HasColumnName("YukumlulukTipId");
            builder.Entity<PropertyPayment>().Property(p => p.Period).HasColumnName("Donem").HasMaxLength(20);
            builder.Entity<PropertyPayment>().Property(p => p.Amount).HasColumnName("Tutar").HasColumnType("decimal(18,2)");
            builder.Entity<PropertyPayment>().Property(p => p.DueDate).HasColumnName("SonOdemeTarihi").HasColumnType("date");
            builder.Entity<PropertyPayment>().Property(p => p.IsPaid).HasColumnName("OdendiMi");
            builder.Entity<PropertyPayment>().Property(p => p.PaymentDate).HasColumnName("OdemeTarihi");
            builder.Entity<PropertyPayment>().Property(p => p.Description).HasColumnName("Aciklama");
            
            builder.Entity<PropertyPayment>()
                .HasOne(p => p.Property)
                .WithMany()
                .HasForeignKey(p => p.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PropertyPayment>()
                .HasOne(p => p.LiabilityType)
                .WithMany(l => l.Payments)
                .HasForeignKey(p => p.LiabilityTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DigitalDocument>().ToTable("tbl_gmk_Dokumanlar");
            builder.Entity<DigitalDocument>().Property(d => d.RelatedTable).HasColumnName("BagliTablo").HasMaxLength(50);
            builder.Entity<DigitalDocument>().Property(d => d.RelatedRecordId).HasColumnName("BagliKayitId");
            builder.Entity<DigitalDocument>().Property(d => d.FilePath).HasColumnName("DosyaYolu");
            builder.Entity<DigitalDocument>().Property(d => d.Extension).HasColumnName("Uzanti").HasMaxLength(10);
            builder.Entity<DigitalDocument>().Property(d => d.CreatedAt).HasColumnName("YuklenmeTarihi");

            // Paket ve Abonelik Konfigürasyonları
            builder.Entity<SubscriptionPackage>().ToTable("tbl_gmk_AbonelikPaketleri");
            builder.Entity<UserSubscription>().ToTable("tbl_gmk_KullaniciAbonelikleri");

            builder.Entity<UserSubscription>()
                .HasOne(us => us.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserSubscription>()
                .HasOne(us => us.Package)
                .WithMany(p => p.UserSubscriptions)
                .HasForeignKey(us => us.PackageId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dijital Sözleşmeler
            builder.Entity<DigitalContract>().ToTable("tbl_gmk_DijitalSozlesmeler");
            
            builder.Entity<DigitalContract>()
                .HasOne(dc => dc.CreatorUser)
                .WithMany()
                .HasForeignKey(dc => dc.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DigitalContract>()
                .HasOne(dc => dc.SecondPartyUser)
                .WithMany()
                .HasForeignKey(dc => dc.SecondPartyUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DigitalContract>()
                .HasOne(dc => dc.Property)
                .WithMany()
                .HasForeignKey(dc => dc.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Bina ve Site Yönetim Modülü İlişkileri ve Decimal Hassasiyetleri
            builder.Entity<BuildingExpense>()
                .Property(be => be.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Building>()
                .HasOne(b => b.ManagerUser)
                .WithMany()
                .HasForeignKey(b => b.ManagerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Building>()
                .HasOne(b => b.District)
                .WithMany()
                .HasForeignKey(b => b.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Building>()
                .HasOne(b => b.City)
                .WithMany()
                .HasForeignKey(b => b.CityId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Entity<Building>()
                .HasOne(b => b.Neighborhood)
                .WithMany()
                .HasForeignKey(b => b.NeighborhoodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BuildingUnit>()
                .HasOne(bu => bu.OwnerUser)
                .WithMany()
                .HasForeignKey(bu => bu.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BuildingUnit>()
                .HasOne(bu => bu.TenantUser)
                .WithMany()
                .HasForeignKey(bu => bu.TenantUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HousingComplex>()
                .HasOne(hc => hc.ManagementCompany)
                .WithMany(mc => mc.ManagedComplexes)
                .HasForeignKey(hc => hc.ManagementCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HousingComplex>()
                .HasOne(hc => hc.City)
                .WithMany()
                .HasForeignKey(hc => hc.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HousingComplex>()
                .HasOne(hc => hc.District)
                .WithMany()
                .HasForeignKey(hc => hc.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HousingComplex>()
                .HasOne(hc => hc.Neighborhood)
                .WithMany()
                .HasForeignKey(hc => hc.NeighborhoodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HousingComplex>()
                .HasOne(hc => hc.Street)
                .WithMany()
                .HasForeignKey(hc => hc.StreetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Building>()
                .HasOne(b => b.HousingComplex)
                .WithMany(hc => hc.Buildings)
                .HasForeignKey(b => b.HousingComplexId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BuildingExpense>()
                .HasOne(be => be.HousingComplex)
                .WithMany()
                .HasForeignKey(be => be.HousingComplexId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UnitDebt>()
                .Property(ud => ud.Amount)
                .HasPrecision(18, 2);

            builder.Entity<UnitDebt>()
                .HasOne(ud => ud.BuildingUnit)
                .WithMany(bu => bu.Debts)
                .HasForeignKey(ud => ud.BuildingUnitId)
                .OnDelete(DeleteBehavior.Restrict); // Cascade delete loop'u engellemek için

            builder.Entity<UnitDebt>()
                .HasOne(ud => ud.BuildingExpense)
                .WithMany()
                .HasForeignKey(ud => ud.BuildingExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // İnşaat Yönetimi İlişkileri (Cascade Delete Loop Önleme)
            builder.Entity<UnitMaterialSelection>()
                .HasOne(ums => ums.SelectedByUser)
                .WithMany()
                .HasForeignKey(ums => ums.SelectedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ConstructionTask>()
                .HasOne(ct => ct.AssignedToUser)
                .WithMany()
                .HasForeignKey(ct => ct.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProjectMaterialCatalog>()
                .Property(pmc => pmc.PriceDifference)
                .HasPrecision(18, 2);

            builder.Entity<Building>()
                .HasOne(b => b.ConstructionProject)
                .WithMany(p => p.Blocks)
                .HasForeignKey(b => b.ConstructionProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Renovation Relationships (Cascade loop önleme)
            builder.Entity<RenovationRequest>()
                .HasOne(rr => rr.User)
                .WithMany()
                .HasForeignKey(rr => rr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RenovationOffer>()
                .HasOne(ro => ro.ServiceProvider)
                .WithMany()
                .HasForeignKey(ro => ro.ServiceProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<RenovationOffer>()
                .Property(ro => ro.Price)
                .HasPrecision(18, 2);

            // B2B MarketPlace Decimal Precision & Relations
            builder.Entity<Quotation>().Property(q => q.LaborCost).HasPrecision(18, 2);
            builder.Entity<Quotation>().Property(q => q.MaterialCost).HasPrecision(18, 2);
            builder.Entity<MaterialPriceInquiry>().Property(m => m.QuotedPrice).HasPrecision(18, 2);
            
            // Financial Architecture Decimal Precision
            builder.Entity<PaymentTransaction>().Property(p => p.Amount).HasPrecision(18, 2);
            builder.Entity<PaymentTransaction>().Property(p => p.PlatformCommissionAmount).HasPrecision(18, 2);
            builder.Entity<PaymentTransaction>().Property(p => p.NetReceiverAmount).HasPrecision(18, 2);
            
            builder.Entity<PlatformCommissionRate>().Property(p => p.Percentage).HasPrecision(18, 2);
            builder.Entity<PlatformCommissionRate>().Property(p => p.FixedFee).HasPrecision(18, 2);
            
            // Short-Term Rental Decimal Precision
            builder.Entity<Reservation>().Property(r => r.TotalPrice).HasPrecision(18, 2);

            // Cascade rules for B2B relations
            builder.Entity<SupplierTradesmanRelation>()
                .HasOne(str => str.SupplierUser)
                .WithMany()
                .HasForeignKey(str => str.SupplierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplierTradesmanRelation>()
                .HasOne(str => str.TradesmanUser)
                .WithMany()
                .HasForeignKey(str => str.TradesmanUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PHASE 8: Support Tickets Cascade rules
            builder.Entity<SupportTicket>()
                .HasOne(st => st.CreatorUser)
                .WithMany()
                .HasForeignKey(st => st.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupportTicket>()
                .HasOne(st => st.AssignedToUser)
                .WithMany()
                .HasForeignKey(st => st.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PHASE 9: Escrow and Financial configurations
            builder.Entity<EscrowTransaction>()
                .HasOne(e => e.SellerUser)
                .WithMany()
                .HasForeignKey(e => e.SellerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<EscrowTransaction>()
                .HasOne(e => e.ReferrerUser)
                .WithMany()
                .HasForeignKey(e => e.ReferrerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure decimal precision for financial tables
            builder.Entity<EscrowTransaction>().Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.PlatformCommissionGross).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.SellerGrossAmount).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.SellerTaxAmount).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.SellerNetAmount).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.ReferrerGrossAmount).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.ReferrerTaxAmount).HasColumnType("decimal(18,2)");
            builder.Entity<EscrowTransaction>().Property(e => e.ReferrerNetAmount).HasColumnType("decimal(18,2)");

            builder.Entity<SystemFund>().Property(f => f.Percentage).HasColumnType("decimal(5,2)");
            builder.Entity<SystemFund>().Property(f => f.Balance).HasColumnType("decimal(18,2)");
            
            builder.Entity<SystemFundTransaction>().Property(t => t.Amount).HasColumnType("decimal(18,2)");
            
            builder.Entity<GlobalFinanceSettings>().Property(s => s.DefaultWithholdingTaxRate).HasColumnType("decimal(5,2)");
            builder.Entity<GlobalFinanceSettings>().Property(s => s.MinimumWithdrawalAmount).HasColumnType("decimal(18,2)");
            
            builder.Entity<UserFinancialProfile>().Property(p => p.CustomWithholdingTaxRate).HasColumnType("decimal(5,2)");

            builder.Entity<PaymentTransaction>()
                .HasOne(pt => pt.SenderUser)
                .WithMany()
                .HasForeignKey(pt => pt.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UniversalSurvey>()
                .HasOne(us => us.SenderUser)
                .WithMany()
                .HasForeignKey(us => us.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UniversalSurvey>()
                .HasOne(us => us.TargetUser)
                .WithMany()
                .HasForeignKey(us => us.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialPriceInquiry>()
                .HasOne(m => m.TradesmanUser)
                .WithMany()
                .HasForeignKey(m => m.TradesmanUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaterialPriceInquiry>()
                .HasOne(m => m.SupplierUser)
                .WithMany()
                .HasForeignKey(m => m.SupplierUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Quotation>()
                .HasOne(q => q.TradesmanUser)
                .WithMany()
                .HasForeignKey(q => q.TradesmanUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================
            // DECIMAL PRECISION CONFIGURATION (COPILOT)
            // ============================================================
            builder.Entity<Property>().Property(p => p.Price).HasPrecision(18, 2);
            builder.Entity<Property>().Property(p => p.Dues).HasPrecision(18, 2);
            builder.Entity<WalletTransaction>().Property(w => w.Amount).HasPrecision(18, 2);
            builder.Entity<UserWalletTransaction>().Property(w => w.Amount).HasPrecision(18, 2);
            builder.Entity<ExchangeRate>().Property(e => e.UsdRate).HasPrecision(18, 4);
            builder.Entity<ExchangeRate>().Property(e => e.EurRate).HasPrecision(18, 4);

            // ============================================================
            // PERFORMANCE INDEXES (COPILOT)
            // ============================================================
            builder.Entity<Property>().HasIndex(p => new { p.IsDeleted, p.StatusId, p.CreatedAt }).HasDatabaseName("IX_Property_Status_Created");
            builder.Entity<Property>().HasIndex(p => new { p.UserId, p.IsDeleted }).HasDatabaseName("IX_Property_User");
            builder.Entity<Property>().HasIndex(p => new { p.BuildingId, p.IsDeleted }).HasDatabaseName("IX_Property_Building");
            builder.Entity<Property>().HasIndex(p => new { p.Price, p.StatusId, p.IsDeleted }).HasDatabaseName("IX_Property_Price_Status");
            
            builder.Entity<Building>().HasIndex(b => new { b.CityId, b.DistrictId, b.NeighborhoodId }).HasDatabaseName("IX_Building_Location");
            builder.Entity<Building>().HasIndex(b => b.StreetId).HasDatabaseName("IX_Building_Street");
            
            builder.Entity<PropertyImage>().HasIndex(p => new { p.PropertyId, p.SortOrder }).HasDatabaseName("IX_PropertyImage_Property_Sort");
            builder.Entity<PropertyImage>().HasIndex(p => p.PropertyId).HasDatabaseName("IX_PropertyImage_Property");
            
            builder.Entity<UserFavorite>().HasIndex(u => new { u.UserId, u.CreatedAt }).HasDatabaseName("IX_UserFavorite_User_Created");
        }
    }
}





