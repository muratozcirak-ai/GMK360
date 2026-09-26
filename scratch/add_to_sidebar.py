import sys

filepath = 'GMK360.Web/Views/Shared/_AdminLayout.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = """<a href="/Definition/Index" class="nav-item @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Definition" ? "active" : "")">
                <i class="ph ph-list-dashes"></i> Genel Tanımlamalar
            </a>"""

replacement = """<a href="/Definition/Index" class="nav-item @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Definition" ? "active" : "")">
                <i class="ph ph-list-dashes"></i> Genel Tanımlamalar
            </a>
            <a href="/Admin/B2bMarketplace" class="nav-item @(ViewContext.RouteData.Values["Action"]?.ToString() == "B2bMarketplace" ? "active" : "")">
                <i class="ph ph-storefront"></i> B2B Pazar Yeri
            </a>"""

content = content.replace(target, replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
