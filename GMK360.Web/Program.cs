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

var builder = WebApplication.CreateBuilder(args);

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
    // DEV: Kolay test için Email Onayı geçici olarak kapatıldı
    options.SignIn.RequireConfirmedAccount = false;
    
    // Güvenlik: Şifre Zorunlulukları
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    
    // Güvenlik: Hesap Kilitleme (Brute-Force Koruması)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Alt Domain (Subdomain) SSO için Cookie Ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    // DEV: Localhost'ta giriş yapabilmek için Cookie.Domain sadece canlı (Production) ortamda set edilmelidir.
    // options.Cookie.Domain = ".gmk360.com"; // Ana ve alt domainlerde ortak oturum
    options.Cookie.Name = "GMK360.AuthCookie";
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

// Güvenlik: Rate Limiting (DDoS ve Kaba Kuvvet Koruması)
builder.Services.AddRateLimiter(options =>
{
    // Genel site trafiği (Saniyede 100 istek)
    options.AddFixedWindowLimiter("GlobalLimiter", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromSeconds(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    // Login ve kritik işlemler (Dakikada 5 istek)
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
builder.Services.AddScoped<IEmailService, GMK360.Core.Services.EmailService>();
builder.Services.AddScoped<IBuildingManagementService, GMK360.Web.Services.BuildingManagementService>();

// Register newly created B2B and Rental Services
builder.Services.AddScoped<GMK360.Core.Interfaces.ITradesmanService, GMK360.Core.Services.TradesmanService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IPaymentGatewayService, GMK360.Core.Services.PaymentGatewayService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IShortTermRentalService, GMK360.Web.Services.ShortTermRentalService>();
builder.Services.AddScoped<GMK360.Core.Interfaces.IInvitationService, GMK360.Web.Services.InvitationService>();
builder.Services.AddHttpClient<INviValidationService, NviValidationService>();
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
builder.Services.AddScoped<GMK360.Web.Services.ISeoService, GMK360.Web.Services.SeoService>();

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
    // Chrome Extension için (SADECE gerçek extension ID'niz varsa)
    options.AddPolicy("AllowExtension", policy =>
    {
        policy.WithOrigins(
                "chrome-extension://YOUR_ACTUAL_EXTENSION_ID_HERE", // Gerçek ID
                "https://localhost:7001",      // Development için
                "https://localhost:5001",      // Development HTTP
                "https://gmk360.com",          // Production domain
                "https://www.gmk360.com"       // Production www
              )
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-TOKEN")
              .AllowCredentials();  // Cookie için gerekli
    });

    // Alt domainler için (kurumsal.gmk360.com gibi)
    options.AddPolicy("SubdomainPolicy", policy =>
    {
        policy.WithOrigins(
                "https://gmk360.com",
                "https://www.gmk360.com",
                "https://kurumsal.gmk360.com",
                "https://*.gmk360.com"  // Tüm alt domainler
              )
              .SetIsOriginAllowedToAllowWildcardSubdomains()
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-TOKEN")
              .AllowCredentials();
    });

    // Development için
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
        // Güvenlik: CSRF (Cross-Site Request Forgery) Koruması tüm POST istekleri için zorunlu
        options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
        
        // Zorunlu Kayıt Tamamlama (Onboarding) Filtresi
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

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GMK360 API v1");
});

app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<GMK360.Web.Middleware.SubdomainRoutingMiddleware>();
app.UseMiddleware<GMK360.Web.Middleware.AgencySubdomainMiddleware>();
app.UseRouting();
app.UseCors("AllowExtension");
app.UseCors("SubdomainPolicy"); // Alt domain CORS politikası aktif edildi

// Enable Localization Middleware
var locOptions = app.Services.GetService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
if (locOptions != null)
{
    app.UseRequestLocalization(locOptions.Value);
}

// Güvenlik Middleware'leri
app.UseRateLimiter(); // Rate limiter devreye alınıyor

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await GMK360.Data.Seeds.RoleAndUserSeeder.SeedRolesAndAdminAsync(services);
        await GMK360.Data.Seeds.DefinitionSeeder.SeedDefinitionsAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Roller ve Admin hesabı oluşturulurken bir hata meydana geldi.");
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





