import io
import re

filepath = r'GMK360.Web\Views\B2BPartnerPortal\Invite.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add the Fleet menu item
fleet_item = """                            <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                <span><i class="bi bi-truck-front-fill text-secondary me-2 fs-5 align-middle"></i> Makine ve Araç Filom</span>
                                <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                            </a>
                            <a href="#" class="list-group-item list-group-item-action d-flex justify-content-between align-items-center py-3 border-0 text-muted">
                                <span><i class="bi bi-fuel-pump-fill text-secondary me-2 fs-5 align-middle"></i> Yakıt (Mazot) Takibi</span>
                                <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>
                            </a>"""

content = content.replace('<span><i class="bi bi-truck text-secondary me-2 fs-5 align-middle"></i> Sefer ve Kantar Fişleri</span>\n                                <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>\n                            </a>', '<span><i class="bi bi-truck text-secondary me-2 fs-5 align-middle"></i> Sefer ve Kantar Fişleri</span>\n                                <span class="pro-badge shadow-sm"><i class="bi bi-lock-fill"></i> PRO</span>\n                            </a>\n' + fleet_item)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Added Fleet and Fuel menus to the right sidebar.")
