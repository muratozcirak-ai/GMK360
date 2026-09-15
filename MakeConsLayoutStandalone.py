import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

# Remove Layout = "_Layout";
content = re.sub(r'@\{\s*Layout\s*=\s*"_Layout";\s*\}', '', content)

# Header prefix
prefix = '''@using Microsoft.AspNetCore.Identity
@using GMK360.Core.Entities.Identity
@inject UserManager<ApplicationUser> UserManager
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - İnşaat ERP</title>
    
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css">
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <script src="https://unpkg.com/@@phosphor-icons/web"></script>
    <style>
        body { background-color: #f8f9fa; }
        .hover-orange:hover { color: #FF6B00 !important; }
        .text-orange { color: #FF6B00; }
        .bg-orange { background-color: #FF6B00; }
        .text-navy { color: #1e3a8a; }
        .bg-navy { background-color: #1e3a8a; }
    </style>
    @await RenderSectionAsync("SEO", required: false)
</head>
<body class="d-flex flex-column min-vh-100 bg-light">
    
    <!-- TOP BAR (FİRMA ÖZEL) -->
    <header class="fixed-top bg-white border-bottom shadow-sm">
        <div class="container-fluid px-4 px-xl-5 d-flex justify-content-between align-items-center py-3">
            
            <!-- SOL: LOGO VE FİRMA ADI -->
            <div class="d-flex align-items-center">
                <a asp-controller="ConstructionProject" asp-action="Index" class="text-decoration-none d-flex align-items-center gap-2">
                    <i class="ph-fill ph-infinity text-orange" style="font-size: 2.2rem;"></i>
                    <span class="fs-3 fw-bold text-navy" style="letter-spacing:-1px;">GMK<span class="text-orange">360</span></span>
                </a>
                
                <span class="fs-3 text-muted mx-3">|</span>
                <i class="bi bi-buildings fs-4 text-primary me-2"></i>
                <span class="fw-bold fs-4 text-uppercase text-dark" style="letter-spacing: 0px;">DENGEHAM İNŞAAT</span>
            </div>
            
            <!-- SAĞ: PROFİL MENÜSÜ -->
            <div class="d-flex gap-3 align-items-center">
                @if (User.Identity.IsAuthenticated)
                {
                    <div class="dropdown">
                        <button class="btn btn-outline-navy px-4 rounded-pill dropdown-toggle d-flex align-items-center gap-2" type="button" data-bs-toggle="dropdown" aria-expanded="false">
                            <i class="ph-fill ph-user-circle fs-5"></i>
                            <span class="d-none d-md-inline">Hesabım</span>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end shadow border-0 mt-2" style="border-radius: 12px; min-width: 200px;">
                            <li class="px-3 py-2 border-bottom mb-1">
                                <div class="fw-bold">@( (await UserManager.GetUserAsync(User))?.DisplayName ?? User.Identity.Name )</div>
                            </li>
                            <li>
                                <form class="form-inline" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })">
                                    <button type="submit" class="dropdown-item text-danger"><i class="ph ph-sign-out me-2"></i>Çıkış Yap</button>
                                </form>
                            </li>
                        </ul>
                    </div>
                }
            </div>
            
        </div>
    </header>

    <!-- ANA İÇERİK (SİDEBAR + CONTENT) -->
    <main class="flex-grow-1" style="margin-top: 80px;">
'''

suffix = '''
    </main>
    
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>'''

# Ensure not to duplicate the using statements that are already in the file
content = content.replace('@using Microsoft.AspNetCore.Identity\r\n', '')
content = content.replace('@using Microsoft.AspNetCore.Identity\n', '')
content = content.replace('@using GMK360.Core.Entities.Identity\r\n', '')
content = content.replace('@using GMK360.Core.Entities.Identity\n', '')
content = content.replace('@inject UserManager<ApplicationUser> UserManager\r\n', '')
content = content.replace('@inject UserManager<ApplicationUser> UserManager\n', '')

# Replace section Scripts
content = re.sub(r'@section Scripts\s*\{\s*@RenderSection\("Scripts", required: false\)\s*\}', '', content)

new_content = prefix + content + suffix

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(new_content)

