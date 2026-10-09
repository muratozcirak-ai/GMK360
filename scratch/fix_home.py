import re

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Fix padding
content = content.replace('padding-top: 8rem !important; padding-bottom: 6rem !important;', 'padding-top: 5rem !important; padding-bottom: 3rem !important;')

# Fix Search Buttons
content = re.sub(r'href="/Home/RoleLanding"(.*?)>.*?lanlar.*? G.*?r</a>', r'href="/PublicRealEstate"\1><i class="bi bi-search me-2"></i> Emlak Portalına Git</a>', content)
content = re.sub(r'href="/Dashboard/ServiceProvider"(.*?)>.*?Usta Bul</a>', r'href="/ServiceProvider"\1><i class="bi bi-search me-2"></i> Usta Bul</a>', content)
content = re.sub(r'href="/Home/RoleLanding"(.*?)>.*?M.*?saitlik Ara</a>', r'href="/Home/GunlukKiralama"\1><i class="bi bi-calendar3 me-2"></i> Müsaitlik Ara</a>', content)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
