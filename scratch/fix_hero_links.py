import re

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the glass cards links and add target="_blank"
content = content.replace('href="/PublicRealEstate"', 'href="/Modules/Emlak" target="_blank"')
content = content.replace('href="/ServiceProvider"', 'href="/Modules/Usta" target="_blank"')
content = content.replace('href="/Home/GunlukKiralama"', 'href="/Modules/KisaDonem" target="_blank"')
content = content.replace('href="/B2BPurchasing/Index"', 'href="/Modules/Tedarik" target="_blank"')

# Add Toplantı Ayarla button
old_paragraph = '<p class="lead text-white-50 mb-5 mx-auto" style="max-width: 700px;">Şantiyenizden site yönetimine, malzeme tedariğinden kira tahsilatına kadar tüm gayrimenkul döngüsünü 360 derece kapalı ekosistemde yönetin.</p>'
new_paragraph = '''<p class="lead text-white-50 mb-4 mx-auto" style="max-width: 700px;">Şantiyenizden site yönetimine, malzeme tedariğinden kira tahsilatına kadar tüm gayrimenkul döngüsünü 360 derece kapalı ekosistemde yönetin.</p>
        <div class="d-flex justify-content-center gap-3 mb-5">
            <a href="/Home/Contact" target="_blank" class="btn btn-outline-light rounded-pill px-4 shadow-sm hover-lift fw-bold"><i class="bi bi-calendar-event me-2"></i> Uzmanla Toplantı Ayarla</a>
        </div>'''
content = content.replace(old_paragraph, new_paragraph)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
