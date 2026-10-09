import re
with open(r'GMK360.Web\Views\Modules\Tedarik.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will add a third bullet point for Cari Takip
old_bullets = '''<li class="mb-3 d-flex"><i class="bi bi-tags-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Toptana Ayrı, Perakendeye Ayrı:</strong> Mülk veya pansiyon sahiplerine perakende (B2C) fiyattan satarken; sisteme kayıtlı müteahhitlere ve büyük şantiyelere otomatik olarak "Pro İskontolu" (B2B) toptan fiyatlarınızı gösterin.</div></li>
                <li class="mb-0 d-flex"><i class="bi bi-megaphone-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Dev Projelerin Tedarikçisi Olun:</strong> Büyük inşaat firmaları 500 metrekare seramik alacağı zaman standart sepet kullanmaz. Sistemin "Teklif İste (RFP)" tahtasına düşen dev hacimli taleplere özel teklifinizi verin, ihaleyi siz kazanın.</div></li>'''

new_bullets = '''<li class="mb-3 d-flex"><i class="bi bi-tags-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Toptana Ayrı, Perakendeye Ayrı:</strong> Mülk sahiplerine perakende satarken; sistemdeki yetkili müteahhitlere otomatik olarak "Pro İskontolu" B2B fiyatlarınızı gösterin.</div></li>
                <li class="mb-3 d-flex"><i class="bi bi-megaphone-fill text-success me-3 fs-5 mt-1"></i> <div><strong>Dev Projelerin Tedarikçisi Olun:</strong> İnşaat firmaları büyük alımlarda standart sepet kullanmaz. "Teklif İste (RFP)" tahtasına düşen dev hacimli taleplere özel teklif verin.</div></li>
                <li class="mb-0 d-flex"><i class="bi bi-journal-text text-success me-3 fs-5 mt-1"></i> <div><strong>SMS'li Cari ve Açık Hesap Takibi:</strong> Müşterilerinize verdiğiniz "açık hesap" (veresiye) malları dijital defterinize işleyin. Bakiye güncellendiğinde veya tahsilat yaklaştığında müşterinize otomatik SMS gitsin, kimseyle borç kavgasına girmeyin.</div></li>'''

content = content.replace(old_bullets, new_bullets)

# Also update the title of that section to include Cari Takip
content = content.replace('<h6 class="text-success fw-bold text-uppercase mb-2">Toptan & Perakende Yönetimi</h6>', '<h6 class="text-success fw-bold text-uppercase mb-2">Finans & B2B Ticaret</h6>')
content = content.replace('<h2 class="fw-bold text-navy mb-4">Çift Katmanlı Fiyatlama ve <br>Dijital İhale (RFP) Tahtası</h2>', '<h2 class="fw-bold text-navy mb-4">Dinamik Fiyatlama, Açık Hesap ve <br>SMS\'li Cari Takibi</h2>')
content = content.replace('Sektörel ticari dengelerinizi bozmayın. Alıcı türüne göre fiyat politikalarınızı dinamik olarak yönetin.', 'Sektörel ticari dengelerinizi bozmayın. Alıcıya göre dinamik fiyat sunun ve açık hesaplarınızı (veresiyeyi) dijital defterinizde hatasız takip edin.')

with open(r'GMK360.Web\Views\Modules\Tedarik.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
