import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'<!-- BARIYER \(FAZ 0\) UYARISI YERINE GERCEK TABLO -->.*?<!-- ŞANTİYE ÖZEL MAL KABUL BÖLÜMÜ -->'

replacement = '''<!-- FAZ 0 ÖZET KARTI -->
  <div class="card border-0 shadow-sm rounded-4 mb-4" style="background: linear-gradient(135deg, #fff5f5 0%, #fff 100%);">
      <div class="card-body p-4 d-flex justify-content-between align-items-center">
          <div>
              <h5 class="fw-bold text-danger mb-1"><i class="bi bi-file-earmark-lock-fill fs-4 me-2"></i> FAZ 0: Yasal Evrak, İzin ve Sözleşme Takibi</h5>
              <p class="text-muted mb-0 ms-4 ps-2">Projenin resmi bürokrasisi, önkoşullar, ruhsatlar ve taşeron ihale araştırmaları bu masadan yönetilmektedir.</p>
          </div>
          <div class="text-end">
              <a href="/PhaseZero/Index/@Model.Id" class="btn btn-danger fw-bold rounded-pill shadow-sm px-4">
                  <i class="bi bi-rocket-takeoff me-2"></i> Faz 0 İhale Masasına Git
              </a>
          </div>
      </div>
  </div>

  <!-- ŞANTİYE ÖZEL MAL KABUL BÖLÜMÜ -->'''

# The original has "<!-- ?ANTYE ZEL MAL KABUL BLM -->" encoding problem?
# Let's search using a regex that handles encoding variations.
target2 = r'<!-- BARIYER \(FAZ 0\) UYARISI YERINE GERCEK TABLO -->.*?<!-- .*?ANT.*?YE .*?ZEL MAL KABUL B.*?L.*?M.*? -->'

replacement2 = '''<!-- FAZ 0 ÖZET KARTI -->
  <div class="card border-0 shadow-sm rounded-4 mb-4 border-danger border-start border-5">
      <div class="card-body p-4 d-flex justify-content-between align-items-center">
          <div>
              <h5 class="fw-bold text-danger mb-1"><i class="bi bi-file-earmark-lock-fill fs-4 me-2"></i> FAZ 0: Yasal Evrak, İzin ve Sözleşme Takibi</h5>
              <p class="text-muted mb-0">Projenin resmi bürokrasisi, önkoşullar, ruhsatlar ve taşeron ihale fizibilitesi "İhale Masası" ekranından yönetilmektedir.</p>
          </div>
          <div class="text-end">
              <a href="/PhaseZero/Index/@Model.Id" class="btn btn-danger fw-bold rounded-pill shadow-sm px-4">
                  <i class="bi bi-rocket-takeoff me-2"></i> Faz 0 İhale Masasına Git
              </a>
          </div>
      </div>
  </div>

  <!-- ŞANTİYE ÖZEL MAL KABUL BÖLÜMÜ -->'''
content = re.sub(target2, replacement2, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)