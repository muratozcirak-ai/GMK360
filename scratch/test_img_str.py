import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. ESKİ BİNA REFACTOR
# We need to change the inner row structure. Currently it's:
# <div class="col-md-12">
#   ... text ...
# </div>
# <!-- ESKİ BİNA FOTO GALERİSİ -->
# ... image code ...

# I will find the col-md-12 and make it col-md-9.
eski_col_start = '''<h6 class="fw-bold text-dark mb-0"><i class="bi bi-building-dash text-danger fs-5 me-2"></i> Eski Bina (Mevcut Durum) Özeti</h6>
            </div>
            <div class="card-body p-4">
                <div class="row g-4">
                    <div class="col-md-12">'''
new_eski_col_start = '''<h6 class="fw-bold text-dark mb-0"><i class="bi bi-building-dash text-danger fs-5 me-2"></i> Eski Bina (Mevcut Durum) Özeti</h6>
            </div>
            <div class="card-body p-3">
                <div class="row g-3">
                    <div class="col-md-8">'''
content = content.replace(eski_col_start, new_eski_col_start)

# Now find the image section and move it to a col-md-4
eski_img_start = '''                                          <div class="col-md-4">
                                              <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                  <i class="bi bi-layers text-secondary fs-4 me-3"></i>
                                                  <div>
                                                      <div class="text-muted small fw-bold">Kat Sayısı</div>
                                                      <div class="fw-bold text-dark">Bodrum Dahil Max @eskiMaxKat Kat</div>
                                                  </div>
                                              </div>
                                          </div>
                                      </div>
                                  </div>
                                  <div class="col-12 mt-2">
                                      <div class="text-muted small"><i class="bi bi-info-circle me-1"></i> Yeni proje için ek dış alan donatıları (Örn: Havuz, Kapalı Otopark) eklenebilir.</div>
                                  </div>
                              </div>
                          </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- ESKİ BİNA FOTO GALERİSİ -->
                        <div class="mt-4 pt-3 border-top">
                            <h6 class="fw-bold mb-3 text-muted" style="font-size: 0.8rem;">Mevcut Durum Görselleri</h6>
                            <div class="d-flex gap-2">'''
# Wait, my previous code had </div> </div> </div> before the FOTO GALERİSİ.
# Let's search exactly what is there.
