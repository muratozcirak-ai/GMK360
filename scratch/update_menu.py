import codecs
import re

path = 'GMK360.Web/Views/Shared/_ConstructionLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

menu_html = '''
                    <div class="list-group-item bg-light fw-bold text-uppercase mt-2" style="font-size: 0.8rem; color: #6c757d;">
                        FİNANS & STRATEJİ
                    </div>
                    <a href="/FinancialStrategy/Index" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Controller"]?.ToString() == "FinancialStrategy" ? "active" : "")">
                        <i class="bi bi-graph-up-arrow me-2 text-primary"></i> Karar Destek & Fizibilite
                    </a>
'''

# We will inject this before '<div class="list-group-item bg-light fw-bold text-uppercase mt-2"' which is currently CRM / SATIS
if '/FinancialStrategy/Index' not in content:
    content = re.sub(
        r'(<div class="list-group-item bg-light fw-bold text-uppercase mt-2" style="font-size: 0.8rem; color: #6c757d;">\s*CRM & SATIŞ)',
        menu_html + r'\n                    \1',
        content
    )
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Menu updated")