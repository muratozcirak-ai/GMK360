import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will write a precise python script to refactor the two blocks.
# Instead of regex over 18KB of Razor code which is dangerous, I will just do simple string replacements of the wrapper HTML.

# 1. Replace ESKİ BİNA wrappers
old_bina_start = '''<!-- ESKİ BİNA (MEVCUT DURUM) -->
    <div class="accordion-item border-0 rounded-4 overflow-hidden mb-3 shadow-sm">
        <h2 class="accordion-header" id="headingOldSummary">
            <button class="accordion-button bg-danger bg-opacity-10 text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOldSummary" aria-expanded="true" aria-controls="collapseOldSummary">
                <i class="bi bi-building-dash text-danger fs-5 me-2"></i> ESKİ BİNA (Mevcut Durum) Özeti
            </button>
        </h2>
        <div id="collapseOldSummary" class="accordion-collapse collapse show" aria-labelledby="headingOldSummary">
            <div class="accordion-body p-4">
                <div class="row g-4">
                    <div class="col-md-8">'''

new_old_bina_start = '''<!-- ESKİ/YENİ KIYASLAMA PANOSU -->
<div class="row g-4 mb-4">
    
    @if (Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Aktif_Santiye || Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Tamamlandi_Teslim)
    {
        <!-- Aktif Şantiyede Eski Bina Gizlenir -->
        <div id="eskiBinaSutunu" class="d-none"></div>
    }
    else
    {
    <!-- ESKİ BİNA (MEVCUT DURUM) -->
    <div class="col-lg-6" id="eskiBinaSutunu">
        <div class="card border-0 rounded-4 shadow-sm h-100 border-top border-danger border-4">
            <div class="card-header bg-white border-bottom-0 pt-3 pb-0 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-building-dash text-danger fs-5 me-2"></i> Eski Bina (Mevcut Durum) Özeti</h6>
            </div>
            <div class="card-body p-4">
                <div class="row g-4">
                    <div class="col-md-12">'''

content = content.replace(old_bina_start, new_old_bina_start)

# 2. Fix the image column for ESKİ BİNA
# The original code has </div></div><div class="col-md-4"> (where the image is).
# We want to change the image from a huge full width to a small gallery thumbnail.
old_image_col = '''                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">'''

new_image_col = '''                                </div>
                            </div>
                        </div>
                        
                        <!-- ESKİ BİNA FOTO GALERİSİ -->
                        <div class="mt-4 pt-3 border-top">
                            <h6 class="fw-bold mb-3 text-muted" style="font-size: 0.8rem;">Mevcut Durum Görselleri</h6>
                            <div class="d-flex gap-2">'''

content = content.replace(old_image_col, new_image_col)

# 3. Modify the image rendering for ESKİ BİNA
old_img = '''<img src="@Model.ExistingBuildingImageUrl" class="img-fluid rounded-4 shadow-sm w-100 h-100 object-fit-cover" alt="Eski Bina" style="min-height: 250px;" />'''
new_img = '''<a href="@Model.ExistingBuildingImageUrl" target="_blank" title="Büyük Halini Gör">
                                <div class="position-relative overflow-hidden rounded-3 border shadow-sm hover-lift" style="width: 80px; height: 80px; cursor: pointer;">
                                    <img src="@Model.ExistingBuildingImageUrl" class="w-100 h-100 object-fit-cover" alt="Eski Bina" />
                                    <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex justify-content-center align-items-center opacity-0 hover-opacity-100 transition">
                                        <i class="bi bi-zoom-in text-white"></i>
                                    </div>
                                </div>
                            </a>'''
content = content.replace(old_img, new_img)

old_img_placeholder = '''<div class="bg-light rounded-4 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted h-100 p-4 border" style="min-height: 250px;">
                            <i class="bi bi-camera fs-1 mb-2"></i>
                            <span>Fotoğraf Yüklenmemiş</span>
                        </div>'''
new_img_placeholder = '''<div class="bg-light rounded-3 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted border" style="width: 80px; height: 80px;">
                                <i class="bi bi-camera"></i>
                            </div>'''
content = content.replace(old_img_placeholder, new_img_placeholder)

# 4. End of ESKİ BİNA and Start of YENİ BİNA
old_bina_end_and_new_start = '''                    </div>
                </div>
            </div>
        </div>
    </div>
    }

    <!-- YENİ (HEDEF) BİNA -->
    <div class="accordion-item border-0 rounded-4 overflow-hidden shadow-sm">
        <h2 class="accordion-header" id="headingNewSummary">
            <button class="accordion-button collapsed bg-primary bg-opacity-10 text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseNewSummary" aria-expanded="false" aria-controls="collapseNewSummary">
                <i class="bi bi-building-add text-primary fs-5 me-2"></i> YENİ (HEDEF) BİNA Özeti
            </button>
        </h2>
        <div id="collapseNewSummary" class="accordion-collapse collapse" aria-labelledby="headingNewSummary">
            <div class="accordion-body p-4">
                <div class="row g-4">
                    <div class="col-md-8">'''

new_bina_start = '''                    </div>
                </div>
            </div>
        </div>
    </div>
    }
    
    <!-- YENİ (HEDEF) BİNA -->
    <!-- If status is Aktif_Santiye, col-lg-12, else col-lg-6 -->
    <div class="@(Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Aktif_Santiye || Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Tamamlandi_Teslim ? "col-lg-12" : "col-lg-6")" id="yeniBinaSutunu">
        <div class="card border-0 rounded-4 shadow-sm h-100 border-top border-primary border-4">
            <div class="card-header bg-white border-bottom-0 pt-3 pb-0 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-building-add text-primary fs-5 me-2"></i> Yeni (Hedef) Bina Özeti</h6>
            </div>
            <div class="card-body p-4">
                <div class="row g-4">
                    <div class="col-md-12">'''

content = content.replace(old_bina_end_and_new_start, new_bina_start)

# 5. Fix Image Column for YENİ BİNA
old_new_image_col = '''                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">'''

new_new_image_col = '''                                </div>
                            </div>
                        </div>
                        
                        <!-- YENİ BİNA FOTO GALERİSİ -->
                        <div class="mt-4 pt-3 border-top">
                            <h6 class="fw-bold mb-3 text-muted" style="font-size: 0.8rem;">Proje/Hedef Görselleri</h6>
                            <div class="d-flex gap-2">'''
                            
content = content.replace(old_new_image_col, new_new_image_col)

# 6. Replace images for YENİ BİNA
old_new_img = '''<img src="@Model.TargetBuildingImageUrl" class="img-fluid rounded-4 shadow-sm w-100 h-100 object-fit-cover" alt="Yeni Bina" style="min-height: 250px;" />'''
new_new_img = '''<a href="@Model.TargetBuildingImageUrl" target="_blank" title="Büyük Halini Gör">
                                <div class="position-relative overflow-hidden rounded-3 border shadow-sm hover-lift" style="width: 80px; height: 80px; cursor: pointer;">
                                    <img src="@Model.TargetBuildingImageUrl" class="w-100 h-100 object-fit-cover" alt="Yeni Bina" />
                                    <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex justify-content-center align-items-center opacity-0 hover-opacity-100 transition">
                                        <i class="bi bi-zoom-in text-white"></i>
                                    </div>
                                </div>
                            </a>'''
content = content.replace(old_new_img, new_new_img)

content = content.replace(old_img_placeholder, new_img_placeholder) # Same placeholder code

# 7. Close the row wrapper
old_end = '''                    </div>
                </div>
            </div>
        </div>
    </div>'''

new_end = '''                    </div>
                </div>
            </div>
        </div>
    </div>
</div> <!-- END OF ROW -->'''

# Need to replace only the specific last closure before "Projedeki Bloklar / Binalar"
# We can just replace the first occurence of old_end after yeniBinaSutunu
yeni_idx = content.find('yeniBinaSutunu')
if yeni_idx != -1:
    end_replace_idx = content.find(old_end, yeni_idx)
    if end_replace_idx != -1:
        content = content[:end_replace_idx] + new_end + content[end_replace_idx + len(old_end):]

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
