import re

filepath = r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

link_to_add = """                    <a href="/Finance/Receipts" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Receipts" ? "active" : "")">
                        <i class="bi bi-receipt me-2 text-info"></i> Masraf Fişleri & Faturalar
                    </a>"""

# Insert it after Ortak Kasa
content = content.replace('<i class="bi bi-bank me-2 text-success"></i> Ortak Kasa (Alacak/Verecek)\n                    </a>', '<i class="bi bi-bank me-2 text-success"></i> Ortak Kasa (Alacak/Verecek)\n                    </a>\n' + link_to_add)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
