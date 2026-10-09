with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Let's extract exactly from @if (Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding)) to the end of the Yeni Bina block.
# I will just write a new View file for the summary part and use partials? No, too complex.

# Let's replace the outer container
accordion_start = '''<div class="accordion mb-4 shadow-sm" id="accordionSummaries">'''
new_accordion_start = '''<!-- ÖZET ALANLARI (ESKİ/YENİ BİNA KIYASLAMA) -->
<div class="row g-4 mb-4" id="accordionSummaries">'''

content = content.replace(accordion_start, new_accordion_start)

# Now, we change the inner accordion items to col-lg-6 cards
eski_item_start = '''<!-- ESKİ BİNA (MEVCUT DURUM) -->
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

new_eski_item_start = '''<!-- ESKİ BİNA (MEVCUT DURUM) -->
    <!-- Aktif Şantiyede Eski Bina gizlenecek mantığı -->
    <div class="@(Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Aktif_Santiye || Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Tamamlandi_Teslim ? "d-none" : "col-lg-6")" id="eskiBinaSutunu">
        <div class="card border-0 rounded-4 shadow-sm h-100 border-top border-danger border-4">
            <div class="card-header bg-white border-bottom-0 pt-3 pb-0 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-building-dash text-danger fs-5 me-2"></i> Eski Bina (Mevcut Durum) Özeti</h6>
            </div>
            <div class="card-body p-4">
                <div class="row g-4">
                    <div class="col-md-12">'''

content = content.replace(eski_item_start, new_eski_item_start)

# Now image for Eski Bina
eski_img_col = '''                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        @if (!string.IsNullOrEmpty(Model.ExistingBuildingImageUrl))
                        {
                            <img src="@Model.ExistingBuildingImageUrl" class="img-fluid rounded-4 shadow-sm w-100 h-100 object-fit-cover" alt="Eski Bina" style="min-height: 250px;" />
                        }
                        else
                        {
                            <div class="bg-light rounded-4 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted h-100 p-4 border" style="min-height: 250px;">
                                <i class="bi bi-camera fs-1 mb-2"></i>
                                <span>Fotoğraf Yüklenmemiş</span>
                            </div>
                        }
                    </div>'''

new_eski_img_col = '''                                </div>
                            </div>
                        </div>
                        
                        <!-- ESKİ BİNA FOTO GALERİSİ -->
                        <div class="mt-4 pt-3 border-top">
                            <h6 class="fw-bold mb-3 text-muted" style="font-size: 0.8rem;">Mevcut Durum Görselleri</h6>
                            <div class="d-flex gap-2">
                                @if (!string.IsNullOrEmpty(Model.ExistingBuildingImageUrl))
                                {
                                    <a href="@Model.ExistingBuildingImageUrl" target="_blank" title="Büyük Halini Gör">
                                        <div class="position-relative overflow-hidden rounded-3 border shadow-sm hover-lift" style="width: 80px; height: 80px; cursor: pointer;">
                                            <img src="@Model.ExistingBuildingImageUrl" class="w-100 h-100 object-fit-cover" alt="Eski Bina" />
                                            <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex justify-content-center align-items-center opacity-0 hover-opacity-100 transition">
                                                <i class="bi bi-zoom-in text-white"></i>
                                            </div>
                                        </div>
                                    </a>
                                }
                                else
                                {
                                    <div class="bg-light rounded-3 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted border" style="width: 80px; height: 80px;">
                                        <i class="bi bi-camera"></i>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>'''
content = content.replace(eski_img_col, new_eski_img_col)


yeni_item_start = '''<!-- YENİ BİNA (HEDEF DURUM) -->
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

new_yeni_item_start = '''<!-- YENİ BİNA (HEDEF DURUM) -->
    <div class="@(Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Aktif_Santiye || Model.Status == GMK360.Core.Entities.Construction.ConstructionProjectStatus.Tamamlandi_Teslim ? "col-lg-12" : "col-lg-6")" id="yeniBinaSutunu">
        <div class="card border-0 rounded-4 shadow-sm h-100 border-top border-primary border-4">
            <div class="card-header bg-white border-bottom-0 pt-3 pb-0 d-flex justify-content-between align-items-center">
                <h6 class="fw-bold text-dark mb-0"><i class="bi bi-building-add text-primary fs-5 me-2"></i> Yeni (Hedef) Bina Özeti</h6>
            </div>
            <div class="card-body p-4">
                <div class="row g-4">
                    <div class="col-md-12">'''
content = content.replace(yeni_item_start, new_yeni_item_start)

yeni_img_col = '''                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        @if (!string.IsNullOrEmpty(Model.TargetBuildingImageUrl))
                        {
                            <img src="@Model.TargetBuildingImageUrl" class="img-fluid rounded-4 shadow-sm w-100 h-100 object-fit-cover" alt="Yeni Bina" style="min-height: 250px;" />
                        }
                        else
                        {
                            <div class="bg-light rounded-4 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted h-100 p-4 border" style="min-height: 250px;">
                                <i class="bi bi-camera fs-1 mb-2"></i>
                                <span>Fotoğraf Yüklenmemiş</span>
                            </div>
                        }
                    </div>'''

new_yeni_img_col = '''                                </div>
                            </div>
                        </div>
                        
                        <!-- YENİ BİNA FOTO GALERİSİ -->
                        <div class="mt-4 pt-3 border-top">
                            <h6 class="fw-bold mb-3 text-muted" style="font-size: 0.8rem;">Proje/Hedef Görselleri</h6>
                            <div class="d-flex gap-2">
                                @if (!string.IsNullOrEmpty(Model.TargetBuildingImageUrl))
                                {
                                    <a href="@Model.TargetBuildingImageUrl" target="_blank" title="Büyük Halini Gör">
                                        <div class="position-relative overflow-hidden rounded-3 border shadow-sm hover-lift" style="width: 80px; height: 80px; cursor: pointer;">
                                            <img src="@Model.TargetBuildingImageUrl" class="w-100 h-100 object-fit-cover" alt="Yeni Bina" />
                                            <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex justify-content-center align-items-center opacity-0 hover-opacity-100 transition">
                                                <i class="bi bi-zoom-in text-white"></i>
                                            </div>
                                        </div>
                                    </a>
                                }
                                else
                                {
                                    <div class="bg-light rounded-3 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted border" style="width: 80px; height: 80px;">
                                        <i class="bi bi-camera"></i>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>'''
content = content.replace(yeni_img_col, new_yeni_img_col)

# Close the accordion wrapper (which is now a row wrapper)
# We don't need to do anything since the closing </div> is structurally identical (an outer wrapper div closing)
# Wait, a div.row closes exactly like a div.accordion.
# Also the accordion items col-lg-6 close exactly like ccordion-item.
# The only issue is div.accordion-collapse and div.accordion-body were removed from my starts, so I MUST remove two </div> tags from the end of each block.

# Let's count the divs I replaced.
# In eski_item_start: I replaced 5 divs (<div accordion-item>, <div accordion-collapse>, <div accordion-body>, <div row>, <div col-md-8>)
# with 5 divs (<div col-lg-6>, <div card>, <div card-body>, <div row>, <div col-md-12>)
# Wait, <h2 class="accordion-header"> and <button> were removed. And I added <div card-header>.
# So 5 divs in, 5 divs out! The closing </div> tags at the bottom will MATCH PERFECTLY!
# Let's verify:
# In old: <div accordion-item> -> <div accordion-collapse> -> <div accordion-body> -> <div row> -> <div col-md-8>
# In new: <div col-lg-6> -> <div card> -> <div card-body> -> <div row> -> <div col-md-12>
# Yes, they map 1-to-1! The closing tags don't need any changes!

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
