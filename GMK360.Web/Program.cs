using GMK360.Data.Contexts;
using GMK360.Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GMK360.Core.Interfaces;
using GMK360.Core.Services;
using GMK360.Data.Repositories;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using GMK360.Web.Services.BackgroundTasks;
using GMK360.Web;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF Lisans Yapılandırması (Community Lisansı)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.Configure<GMK360.Core.Settings.SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.Configure<GMK360.Core.Settings.MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Configure maximum request limits for Video Uploads (100MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100 MB
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
    // DEV: Kolay test iÃƒÂ§in Email OnayÃ„Â± geÃƒÂ§ici olarak kapatÃ„Â±ldÃ„Â±
    options.SignIn.RequireConfirmedAccount = false;
    
    // GÃƒÂ¼venlik: Ã…Âifre ZorunluluklarÃ„Â±
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    
    // GÃƒÂ¼venlik: Hesap Kilitleme (Brute-Force KorumasÃ„Â±)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// IIS / Plesk restart sonrası oturumun ve "Beni Hatırla" çerezinin düşmemesi için kalıcı DataProtection anahtarı
var dpKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dpKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeysPath))
    .SetApplicationName("GMK360");

// Alt Domain (Subdomain) SSO için Cookie Ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    // DEV: Localhost'ta giriş yapabilmek için Cookie.Domain sadece canlı (Production) ortamda set edilmelidir.
    // options.Cookie.Domain = ".gmk360.com"; // Ana ve alt domainlerde ortak oturum
    options.Cookie.Name = "GMK360.AuthCookie";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var authBuilder = builder.Services.AddAuthentication();

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Google:ClientId"]))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    });
}

if (!string.IsNullOrEmpty(builder.Configuration["Authentication:Apple:ClientId"]))
{
    authBuilder.AddApple(options => 
    {
        options.ClientId = builder.Configuration["Authentication:Apple:ClientId"]!;
        options.KeyId = builder.Configuration["Authentication:Apple:KeyId"] ?? "dummy_key";
        options.TeamId = builder.Configuration["Authentication:Apple:TeamId"] ?? "dummy_team";
        options.UsePrivateKey((keyId) => 
            builder.Environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
    });
}

// GÃƒÂ¼venlik: Rate Limiting (DDoS ve Kaba Kuvvet KorumasÃ„Â±)
builder.Services.AddRateLimiter(options =>
{
    // Genel site trafiÃ„Å¸i (Saniyede 100 istek)
    options.AddFixedWindowLimiter("GlobalLimiter", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Login ve kritik iÃ…Å¸lemler (Dakikada 5 istek)
    options.AddFixedWindowLimiter("LoginLimiter", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// Register Repositories and Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPropertyService, PropertyService>();
// Add global HttpClientFactory
builder.Services.AddHttpClient();

builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IEmailService, GMK360.Web.Services.DbAwareEmailService>();
builder.Services.AddScoped<IBuildingManagementService, GMK360.Web.Services.BuildingManagementService>();
builder.Services.AddScoped<GMK360.Web.Services.FacilityGovernanceService>();
builder.Services.AddScoped<GMK360.Web.Services.FacilityPayrollAndBudgetService>();
builder.Services.AddScoped<GMK360.Web.Services.FacilityTaskDispatchService>();
builder.Services.AddScoped<GMK360.Web.Services.SaasSubscriptionService>();

// Register newly created B2B and Rental Services
builder.Services.AddScoped<GMK360.Core.Interfaces.ITradesmanService, GMK360.Core.Services.TradesmanService>();
builder.Services.AddScoped<GMK360.Web.Services.IUstaWorkspaceService, GMK360.Web.Services.UstaWorkspaceService>();
builder.Services.AddScoped<GMK360.Web.Services.ITaseronWorkspaceService, GMK360.Web.Services.TaseronWorkspaceService>();
builder.Services.AddScoped<GMK360.Web.Services.ITedarikciWorkspaceService, GMK360.Web.Services.TedarikciWorkspaceService>();
builder.Services.AddScoped<GMK360.Web.Services.IKurumsalB2BWorkspaceService, GMK360.Web.Services.KurumsalB2BWorkspaceService>();
builder.Services.AddScoped<GMK360.Web.Services.IRealtorWorkspaceService, GMK360.Web.Services.RealtorWorkspaceService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IPaymentGatewayService, GMK360.Core.Services.PaymentGatewayService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IShortTermRentalService, GMK360.Web.Services.ShortTermRentalService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IInvitationService, GMK360.Web.Services.InvitationService>();
builder.Services.AddHttpClient<INviValidationService, NviValidationService>();
builder.Services.AddHttpClient<ITkgmService, GMK360.Web.Services.TkgmService>();
builder.Services.AddScoped<GMK360.Web.Services.WalletService>();
builder.Services.AddScoped<IPropertySearchService, GMK360.Web.Services.PropertySearchService>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, GMK360.Web.Services.MockEmailSender>();
builder.Services.AddScoped<GMK360.Web.Services.Ocr.IOcrService, GMK360.Web.Services.Ocr.TesseractOcrService>();
builder.Services.AddScoped<GMK360.Web.Services.AI.IAiImageLabelingService, GMK360.Web.Services.AI.PropertyImageLabelingService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IGeminiAiService, GMK360.Web.Services.AI.GeminiAiManager>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IWebhookService, GMK360.Web.Services.N8nWebhookManager>();
builder.Services.AddScoped<GMK360.Web.Services.Sms.ISmsService, GMK360.Web.Services.Sms.SmsService>();
builder.Services.AddScoped<GMK360.Web.Services.IAppointmentService, GMK360.Web.Services.AppointmentService>();
builder.Services.AddScoped<GMK360.Core.Services.IYetkiBelgesiServisi, GMK360.Core.Services.TestYetkiBelgesiServisi>();
builder.Services.AddHttpClient<GMK360.Core.Services.IEfaturaServisi, GMK360.Core.Services.MerkezEfaturaServisi>();
builder.Services.AddScoped<GMK360.Web.Services.Integration.KbsIntegrationService>();
builder.Services.AddScoped<GMK360.Web.Services.Integration.IyzicoPaymentService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.INotificationService, GMK360.Web.Services.NotificationService>();
builder.Services.AddSingleton<GMK360.Web.Routing.SeoRouteTransformer>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IFinancialEngineService, GMK360.Web.Services.FinancialEngineService>();
builder.Services.AddScoped<GMK360.Web.Services.IValuationService, GMK360.Web.Services.ValuationService>();
builder.Services.AddScoped<GMK360.Web.Services.B2BSupplierSyncService>();
builder.Services.AddScoped<GMK360.Web.Services.ISeoService, GMK360.Web.Services.SeoService>();
builder.Services.AddScoped<GMK360.Web.Services.Construction.DnaTimeOffsetService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IConstructionFeasibilityPdfService, GMK360.Web.Services.Construction.ConstructionFeasibilityPdfService>();

// Register Background Services
builder.Services.AddHostedService<LocationIntelligenceWorker>();
builder.Services.AddHostedService<TcmbExchangeRateService>();
builder.Services.AddHostedService<ICalSyncService>();
builder.Services.AddHostedService<GMK360.Web.Services.TempMediaCleanupService>();
builder.Services.AddHostedService<GMK360.Web.Services.BackgroundTasks.EidsVerificationWorker>();
builder.Services.AddHostedService<GMK360.Web.Services.BackgroundTasks.AuthorizationExpiryWorker>();
builder.Services.AddHostedService<GMK360.Web.Services.DailyReminderService>();

// Localization Configuration
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Add CORS Policy for Chrome Extension and Subdomains
builder.Services.AddCors(options =>
{
    // Not: Chrome Extension CORS politikasi, gercek extension ID alindiginda buraya eklenecektir.
    // options.AddPolicy("AllowExtension", policy => { ... });


    // Alt domainler iÃƒÂ§in (kurumsal.gmk360.com gibi)
    options.AddPolicy("SubdomainPolicy", policy =>
    {
        policy.WithOrigins(
                "https://gmk360.com",
                "https://www.gmk360.com",
                "https://kurumsal.gmk360.com",
                "https://*.gmk360.com"  // TÃƒÂ¼m alt domainler
              )
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-TOKEN")
              .AllowCredentials();
    });

    // Development iÃƒÂ§in
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowDevelopment", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Vite, React
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

builder.Services.AddControllersWithViews(options => 
    {
        // GÃƒÂ¼venlik: CSRF (Cross-Site Request Forgery) KorumasÃ„Â± tÃƒÂ¼m POST istekleri iÃƒÂ§in zorunlu
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
        
        // Zorunlu KayÃ„Â±t Tamamlama (Onboarding) Filtresi
        options.Filters.Add(typeof(GMK360.Web.Filters.OnboardingRequirementFilter));
    })
    .AddRazorRuntimeCompilation()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("tr-TR"),
        new CultureInfo("en-US"),
        new CultureInfo("ru-RU")
    };

    options.DefaultRequestCulture = new RequestCulture("tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddScoped<GMK360.Web.Services.Construction.ProjectManagerService>();

var app = builder.Build();

// Enable Swagger UI (sadece Development ortaminda)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GMK360 API v1");
    });
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<GMK360.Web.Middleware.SubdomainRoutingMiddleware>();
app.UseMiddleware<GMK360.Web.Middleware.AgencySubdomainMiddleware>();
app.UseRouting();

// CORS: Tek middleware cagrisi ile tum politikalari yonet
// Not: Controller/endpoint bazinda [EnableCors("PolicyName")] attribute ile ayrica politika atanabilir
app.UseCors("SubdomainPolicy");

// Enable Localization Middleware
var locOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
if (locOptions != null)
{
    app.UseRequestLocalization(locOptions.Value);
}

// GÃƒÂ¼venlik Middleware'leri
app.UseRateLimiter(); // Rate limiter devreye alÃ„Â±nÃ„Â±yor

app.UseAuthentication();
app.UseAuthorization();

// Production ortaminda mevcut veritabanina otomatik mudehale edilmemesi icin
// yalnizca Development modunda veya RunStartupDbInit=true ayarlandiginda calisir.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("RunStartupDbInit"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<GMK360.Data.Contexts.ApplicationDbContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Otomatik migration sırasında uyarı (manuel yüklenmiş DB ise normaldir).");
        }

        try
        {
            await GMK360.Data.Seeds.RoleAndUserSeeder.SeedRolesAndAdminAsync(services);
            await GMK360.Data.Seeds.DefinitionSeeder.SeedDefinitionsAsync(services);
            
            // Demo verileri sadece Development ortaminda yuklenir
            if (app.Environment.IsDevelopment())
            {
                await GMK360.Data.Seeds.DemoSeeder.SeedDemoDataAsync(services);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Roller ve Admin hesabı oluşturulurken bir hata meydana geldi.");
        }
    }
}

// Production dahil tüm ortamlarda sadece eksik kalan yeni kolon/tabloları güvenli ve idempotent (veri kayıpsız) tamamla
using (var schemaScope = app.Services.CreateScope())
{
    var schemaLogger = schemaScope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var schemaDb = schemaScope.ServiceProvider.GetRequiredService<GMK360.Data.Contexts.ApplicationDbContext>();
    try
    {
        schemaDb.Database.ExecuteSqlRaw(@"
            IF COL_LENGTH(N'dbo.HousingComplexes', N'IsIndependentSite') IS NULL
                ALTER TABLE [dbo].[HousingComplexes] ADD [IsIndependentSite] bit NOT NULL CONSTRAINT [DF_HousingComplexes_IsIndependentSite] DEFAULT (1);
            IF COL_LENGTH(N'dbo.HousingComplexes', N'Subdomain') IS NULL
                ALTER TABLE [dbo].[HousingComplexes] ADD [Subdomain] nvarchar(100) NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'Subdomain') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [Subdomain] nvarchar(100) NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DefaultBaseMonthlyDues') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DefaultBaseMonthlyDues] decimal(18,2) NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DuesPaymentDay') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DuesPaymentDay] int NULL;
            IF COL_LENGTH(N'dbo.BuildingUnits', N'IsDuesExempt') IS NULL
                ALTER TABLE [dbo].[BuildingUnits] ADD [IsDuesExempt] bit NOT NULL CONSTRAINT [DF_BuildingUnits_IsDuesExempt] DEFAULT (0);
            IF COL_LENGTH(N'dbo.BuildingUnits', N'DuesAdjustmentAmount') IS NULL
                ALTER TABLE [dbo].[BuildingUnits] ADD [DuesAdjustmentAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_BuildingUnits_DuesAdjustmentAmount] DEFAULT (0);
            IF COL_LENGTH(N'dbo.BuildingExpenses', N'PhysicalArchiveLocation') IS NULL
                ALTER TABLE [dbo].[BuildingExpenses] ADD [PhysicalArchiveLocation] nvarchar(250) NULL;
            IF COL_LENGTH(N'dbo.BuildingExpenses', N'LinkedSourceType') IS NULL
                ALTER TABLE [dbo].[BuildingExpenses] ADD [LinkedSourceType] nvarchar(40) NULL;
            IF COL_LENGTH(N'dbo.BuildingExpenses', N'LinkedTaskTicketId') IS NULL
                ALTER TABLE [dbo].[BuildingExpenses] ADD [LinkedTaskTicketId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingExpenses', N'LinkedFixedExpenseId') IS NULL
                ALTER TABLE [dbo].[BuildingExpenses] ADD [LinkedFixedExpenseId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingExpenses', N'LinkedSourceLabel') IS NULL
                ALTER TABLE [dbo].[BuildingExpenses] ADD [LinkedSourceLabel] nvarchar(250) NULL;
            IF OBJECT_ID(N'dbo.PlatformFeedbacks', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[PlatformFeedbacks] (
                    [Id] int IDENTITY(1,1) NOT NULL,
                    [ModuleArea] nvarchar(60) NOT NULL,
                    [FeedbackType] nvarchar(40) NOT NULL,
                    [Subject] nvarchar(200) NOT NULL,
                    [Message] nvarchar(4000) NOT NULL,
                    [PageUrl] nvarchar(500) NULL,
                    [DeviceInfo] nvarchar(300) NULL,
                    [UserId] nvarchar(450) NULL,
                    [UserFullName] nvarchar(150) NULL,
                    [UserEmail] nvarchar(200) NULL,
                    [UserRoleLabel] nvarchar(100) NULL,
                    [ReferenceEntityId] int NULL,
                    [Status] nvarchar(30) NOT NULL,
                    [AdminNote] nvarchar(2000) NULL,
                    [ResolvedAt] datetime2 NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_PlatformFeedbacks] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_PlatformFeedbacks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
                );
            END
            IF OBJECT_ID(N'dbo.SystemEmailSettings', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[SystemEmailSettings] (
                    [Id] int IDENTITY(1,1) NOT NULL,
                    [Host] nvarchar(150) NOT NULL,
                    [Port] int NOT NULL,
                    [Mail] nvarchar(200) NOT NULL,
                    [Password] nvarchar(500) NOT NULL,
                    [SenderName] nvarchar(150) NOT NULL,
                    [EnableSsl] bit NOT NULL,
                    [IgnoreCertificateErrors] bit NOT NULL,
                    [IsActive] bit NOT NULL,
                    [LastTestAt] datetime2 NULL,
                    [LastTestStatus] nvarchar(500) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_SystemEmailSettings] PRIMARY KEY ([Id])
                );
            END
            IF COL_LENGTH(N'dbo.AspNetUsers', N'Address') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [Address] nvarchar(500) NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressCityId') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressCityId] int NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressDistrictId') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressDistrictId] int NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressNeighborhoodId') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressNeighborhoodId] int NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressStreetId') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressStreetId] int NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressStreetName') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressStreetName] nvarchar(200) NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressBuildingNo') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressBuildingNo] nvarchar(40) NULL;
            IF COL_LENGTH(N'dbo.AspNetUsers', N'AddressApartmentNo') IS NULL
                ALTER TABLE [dbo].[AspNetUsers] ADD [AddressApartmentNo] nvarchar(30) NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'Email') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [Email] nvarchar(200) NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'Address') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [Address] nvarchar(500) NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'IsSiteWide') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [IsSiteWide] bit NOT NULL CONSTRAINT [DF_BuildingStaffs_IsSiteWide] DEFAULT (0);
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'HousingComplexId') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [HousingComplexId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressCityId') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressCityId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressDistrictId') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressDistrictId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressNeighborhoodId') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressNeighborhoodId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressStreetId') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressStreetId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressStreetName') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressStreetName] nvarchar(200) NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressBuildingNo') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressBuildingNo] nvarchar(40) NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'AddressApartmentNo') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [AddressApartmentNo] nvarchar(30) NULL;
            IF COL_LENGTH(N'dbo.SystemMeetings', N'IsSiteWide') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [IsSiteWide] bit NOT NULL CONSTRAINT [DF_SystemMeetings_IsSiteWide] DEFAULT (0);
            IF COL_LENGTH(N'dbo.SystemMeetings', N'HousingComplexId') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [HousingComplexId] int NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'JobDescription') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [JobDescription] nvarchar(300) NULL;
            IF COL_LENGTH(N'dbo.BuildingStaffs', N'TerminationReason') IS NULL
                ALTER TABLE [dbo].[BuildingStaffs] ADD [TerminationReason] nvarchar(200) NULL;
            IF OBJECT_ID(N'dbo.BuildingStaffDocuments', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[BuildingStaffDocuments] (
                    [Id] int IDENTITY(1,1) NOT NULL,
                    [BuildingStaffId] int NOT NULL,
                    [DocumentType] nvarchar(60) NOT NULL,
                    [Title] nvarchar(200) NOT NULL,
                    [FileUrl] nvarchar(500) NULL,
                    [PhysicalArchiveLocation] nvarchar(250) NULL,
                    [IssueDate] datetime2 NULL,
                    [ExpiryDate] datetime2 NULL,
                    [IsVerified] bit NOT NULL,
                    [Notes] nvarchar(1000) NULL,
                    [UploadedByUserId] nvarchar(450) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_BuildingStaffDocuments] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_BuildingStaffDocuments_BuildingStaffs_BuildingStaffId] FOREIGN KEY ([BuildingStaffId]) REFERENCES [dbo].[BuildingStaffs] ([Id]) ON DELETE CASCADE
                );
            END
            IF COL_LENGTH(N'dbo.SystemMeetings', N'DecisionNumber') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [DecisionNumber] nvarchar(40) NULL;
            IF COL_LENGTH(N'dbo.SystemMeetings', N'MeetingLocation') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [MeetingLocation] nvarchar(200) NULL;
            IF COL_LENGTH(N'dbo.SystemMeetings', N'QuorumType') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [QuorumType] nvarchar(30) NOT NULL CONSTRAINT [DF_SystemMeetings_QuorumType] DEFAULT (N'Standard');
            IF COL_LENGTH(N'dbo.SystemMeetings', N'DecisionCategory') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [DecisionCategory] nvarchar(40) NOT NULL CONSTRAINT [DF_SystemMeetings_DecisionCategory] DEFAULT (N'General');
            IF COL_LENGTH(N'dbo.SystemMeetings', N'ProposedPayloadJson') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [ProposedPayloadJson] nvarchar(max) NULL;
            IF COL_LENGTH(N'dbo.SystemMeetings', N'AutoExecuteOnQuorum') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [AutoExecuteOnQuorum] bit NOT NULL CONSTRAINT [DF_SystemMeetings_AutoExecuteOnQuorum] DEFAULT (1);
            IF COL_LENGTH(N'dbo.SystemMeetings', N'IsExecuted') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [IsExecuted] bit NOT NULL CONSTRAINT [DF_SystemMeetings_IsExecuted] DEFAULT (0);
            IF COL_LENGTH(N'dbo.SystemMeetings', N'ExecutedAt') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [ExecutedAt] datetime2 NULL;
            IF COL_LENGTH(N'dbo.SystemMeetings', N'ExecutionSummary') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [ExecutionSummary] nvarchar(500) NULL;
            IF COL_LENGTH(N'dbo.SystemMeetings', N'VerificationToken') IS NULL
                ALTER TABLE [dbo].[SystemMeetings] ADD [VerificationToken] nvarchar(40) NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DecisionBookNotaryName') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DecisionBookNotaryName] nvarchar(150) NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DecisionBookJournalNo') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DecisionBookJournalNo] nvarchar(80) NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DecisionBookOpenDate') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DecisionBookOpenDate] datetime2 NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DecisionBookLastClosingDate') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DecisionBookLastClosingDate] datetime2 NULL;
            IF COL_LENGTH(N'dbo.Buildings', N'DecisionBookTotalPages') IS NULL
                ALTER TABLE [dbo].[Buildings] ADD [DecisionBookTotalPages] int NULL;
            IF OBJECT_ID(N'dbo.BuildingDecisionVotes', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[BuildingDecisionVotes] (
                    [Id] int IDENTITY(1,1) NOT NULL,
                    [SystemMeetingId] int NOT NULL,
                    [BuildingUnitId] int NOT NULL,
                    [VoterUserId] nvarchar(450) NULL,
                    [VoterName] nvarchar(150) NOT NULL,
                    [ParticipationType] nvarchar(25) NOT NULL,
                    [ProxyPersonName] nvarchar(150) NULL,
                    [ProxyDocumentUrl] nvarchar(500) NULL,
                    [VoteChoice] nvarchar(20) NOT NULL,
                    [DissentNote] nvarchar(1000) NULL,
                    [UnitLandShareSnapshot] decimal(18,4) NOT NULL,
                    [ApprovalTraceCode] nvarchar(40) NULL,
                    [VotedAt] datetime2 NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_BuildingDecisionVotes] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_BuildingDecisionVotes_SystemMeetings_SystemMeetingId] FOREIGN KEY ([SystemMeetingId]) REFERENCES [dbo].[SystemMeetings] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_BuildingDecisionVotes_BuildingUnits_BuildingUnitId] FOREIGN KEY ([BuildingUnitId]) REFERENCES [dbo].[BuildingUnits] ([Id]) ON DELETE CASCADE
                );
            END
            IF OBJECT_ID(N'dbo.BuildingDecisionAttachments', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[BuildingDecisionAttachments] (
                    [Id] int IDENTITY(1,1) NOT NULL,
                    [SystemMeetingId] int NOT NULL,
                    [BuildingUnitId] int NULL,
                    [AuthorUserId] nvarchar(450) NULL,
                    [AuthorName] nvarchar(150) NOT NULL,
                    [AuthorRoleLabel] nvarchar(40) NOT NULL,
                    [CommentText] nvarchar(2000) NULL,
                    [DocumentTitle] nvarchar(200) NULL,
                    [FileUrl] nvarchar(500) NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [UpdatedAt] datetime2 NULL,
                    [IsDeleted] bit NOT NULL,
                    CONSTRAINT [PK_BuildingDecisionAttachments] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_BuildingDecisionAttachments_SystemMeetings_SystemMeetingId] FOREIGN KEY ([SystemMeetingId]) REFERENCES [dbo].[SystemMeetings] ([Id]) ON DELETE CASCADE
                );
            END
        ");
    }
    catch (Exception schemaEx)
    {
        schemaLogger.LogWarning(schemaEx, "Startup idempotent schema kontrolü uyarısı.");
    }
}


// SEO Dostu Yönlendirmeler (Slug tabanlı)

app.MapDynamicControllerRoute<GMK360.Web.Routing.SeoRouteTransformer>(
    "{city}/{district}/{seoSlug}");

app.MapControllerRoute(
    name: "blog_seo",
    pattern: "blog/{seoSlug}",
    defaults: new { controller = "Blog", action = "DetailBySlug" });

app.MapAreaControllerRoute(
    name: "MyArea",
    areaName: "GMK360",
    pattern: "GMK360/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();









// Trigger Rebuild


