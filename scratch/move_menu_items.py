import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Extract items
garaj_item = '<a href="/CompanyVehicles/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-warning"></i> Şirket Garajı & Araçlar</a>'
gider_item = '<a href="/ConstructionProjectExpenses/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-danger"></i> Şirket Genel Giderleri</a>'

# Remove them from their current locations
content = content.replace(garaj_item, '')
content = content.replace(gider_item, '')

# Change icon color to match destination
garaj_item_new = '<a href="/CompanyVehicles/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-primary"></i> Şirket Garajı & Araçlar</a>'
gider_item_new = '<a href="/ConstructionProjectExpenses/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-danger"></i> Şirket Genel Giderleri</a>'

# Insert garaj_item into "ŞANTİYE & PROJELER"
# Find insertion point: <a href="/Inventory/Index"...>...</a>
inventory_item = '<a href="/Inventory/Index" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-primary"></i> Merkez Depo (Demirbaş)</a>'
content = content.replace(inventory_item, inventory_item + '\n                                    ' + garaj_item_new)

# Insert gider_item into "FİNANS & CARİ"
# Find insertion point: <a href="/Finance/UpcomingPayments"...>...</a>
vadesi_item = '<a href="/Finance/UpcomingPayments" class="list-group-item list-group-item-action border-0 ps-5"><i class="bi bi-arrow-right-short text-warning"></i> Vadesi Yaklaşanlar</a>'
content = content.replace(vadesi_item, vadesi_item + '\n                                    ' + gider_item_new)


# Clean up any leftover empty lines inside list-group if any, but string replace should be clean enough

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
