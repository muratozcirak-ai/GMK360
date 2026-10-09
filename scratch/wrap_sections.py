import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# We need to wrap everything from <!-- PROJE VİTALLERİ to right before <!-- ÖZET ALANLARI

# Let's find the start index
start_idx = content.find('<!-- PROJE V')
if start_idx == -1:
    print("Could not find start index")
    exit()

end_idx = content.find('<!-- ÖZET ALANLARI (COLLAPSIBLE) -->')
if end_idx == -1:
    end_idx = content.find('<!-- ZET ALANLARI (COLLAPSIBLE) -->')
if end_idx == -1:
    end_idx = content.find('id="accordionOzet"')
    if end_idx != -1:
        # backup slightly
        end_idx = content.rfind('<!--', 0, end_idx)

if end_idx == -1:
    print("Could not find end index")
    exit()

wrapper_start = '''
@if (Model.Status != GMK360.Core.Entities.Construction.ProjectStatus.Projelendirme_Teklif)
{
    <!-- PROJE VİTALLERİ VE AKTİF ŞANTİYE GÖRÜNÜMLERİ SADECE PROJE BAŞLAYINCA GÖRÜNÜR -->
'''
wrapper_end = '''
}
else
{
    <!-- ADAY PROJE (PROJELENDİRME / TEKLİF) DURUMUNDA GÖSTERİLECEK BİLGİ MESAJI -->
    <div class="alert alert-info border-0 rounded-4 shadow-sm mb-4 d-flex align-items-center">
        <i class="bi bi-info-circle-fill fs-3 me-3 text-info"></i>
        <div>
            <h6 class="fw-bold mb-1">Proje Henüz Teklif / Fizibilite Aşamasında</h6>
            <p class="mb-0 small text-muted">Şantiye ilerleme durumu, finansal veriler ve personel tabloları, proje durumu "Anlaşma Yapıldı" veya "Aktif Şantiye" konumuna alındığında aktifleşecektir. Müşteri sunumu için şu an gizlidir.</p>
        </div>
    </div>
}

'''

new_content = content[:start_idx] + wrapper_start + content[start_idx:end_idx] + wrapper_end + content[end_idx:]

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(new_content)

print("Wrapped the 3 sections successfully.")
