import codecs
import re

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the giant grid with a compact grid
target = r'<!-- PROJE FAZLARI VE ANA YÖNETİM \(9 AŞAMA\) -->.*?<style>.*?\.transition-hover:hover \{ transform: translateY\(-3px\); box-shadow: 0 \.5rem 1rem rgba\(0,0,0,\.15\)!important; \}.*?</style>'

replacement = '''<!-- PROJE FAZLARI VE ANA YÖNETİM (KİBAR LİSTE) -->
  <div class="mb-4">
      <h5 class="fw-bold text-dark mb-3"><i class="bi bi-diagram-3-fill text-primary me-2"></i> Proje Fazları (Aşama Yönetimi)</h5>
      
      <div class="row g-2">
          <!-- FAZ 0 -->
          <div class="col-md-4 col-lg-3">
              <a href="/PhaseZero/Index/@Model.Id" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-danger border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-danger bg-opacity-10 text-danger rounded p-2 me-2">
                          <i class="bi bi-file-earmark-lock-fill"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 0</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Resmi Evrak & İhale</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-danger small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 1 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-cone-striped"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 1</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Yıkım ve Zemin</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 2 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-bricks"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 2</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Temel ve Alt Yapı</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 3 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-building"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 3</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Kaba İnşaat</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 4 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-house-door"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 4</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Çatı ve Dış Cephe</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 5 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-paint-bucket"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 5</h6>
                          <small class="text-muted" style="font-size:0.75rem;">İnce İşler</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 6 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-lightning-charge"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 6</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Elektrik</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 7 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-primary border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-primary bg-opacity-10 text-primary rounded p-2 me-2">
                          <i class="bi bi-gear"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 7</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Mekanik</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-primary small"></i>
                  </div>
              </a>
          </div>

          <!-- FAZ 8 -->
          <div class="col-md-4 col-lg-3">
              <a href="#" class="card border-0 shadow-sm text-decoration-none transition-hover h-100 border-success border-start border-3">
                  <div class="card-body p-2 d-flex align-items-center">
                      <div class="bg-success bg-opacity-10 text-success rounded p-2 me-2">
                          <i class="bi bi-tree"></i>
                      </div>
                      <div class="lh-1">
                          <h6 class="mb-1 fw-bold text-dark" style="font-size:0.85rem;">FAZ 8</h6>
                          <small class="text-muted" style="font-size:0.75rem;">Peyzaj ve Teslim</small>
                      </div>
                      <i class="bi bi-chevron-right ms-auto text-success small"></i>
                  </div>
              </a>
          </div>
      </div>
  </div>
  
  <style>
      .transition-hover { transition: transform 0.1s, box-shadow 0.1s; }
      .transition-hover:hover { transform: translateY(-2px); box-shadow: 0 .25rem .5rem rgba(0,0,0,.1)!important; }
  </style>'''

content = re.sub(target, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)