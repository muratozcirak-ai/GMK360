import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# Replace the old layout with the new accordion layout
# Find the exact old block in pristine:
# It starts at: <div class="row g-4 mb-4"> (where Proje Özeti is)
# And ends before: <!-- TASLAK
pattern = re.compile(r'<div class="row g-4 mb-4">.*?<!-- TASLAK', re.DOTALL)

clean_accordion = r'''<!-- HARİTA VE ÇEVRE ÖZELLİKLERİ (BAĞIMSIZ) -->
<div class="row g-4 mb-4">
    <div class="col-12">
        <div class="card border-0 shadow-sm rounded-4">
            <div class="card-body p-4 d-flex justify-content-between align-items-center flex-wrap gap-3">
                <div class="d-flex align-items-center">
                    <i class="bi bi-geo-alt-fill text-danger fs-1 me-3"></i>
                    <div>
                        <h5 class="fw-bold mb-1">Konum Verileri ve Çevre Özellikleri</h5>
                        <p class="text-muted small mb-0">Google Haritalar (Places API) üzerinden hastane, okul, park gibi çevre özellikleri buraya otomatik çekilecektir.</p>
                    </div>
                </div>
                <div class="d-flex gap-2">
                    @if(Model.Latitude != null && Model.Longitude != null)
                    {
                        <a href="https://www.google.com/maps?q=@Model.Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),@Model.Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)" target="_blank" class="btn btn-outline-danger rounded-pill shadow-sm">
                            <i class="bi bi-geo-alt-fill me-1"></i> Lojistik Haritası
                        </a>
                    }
                    <button type="button" class="btn btn-outline-secondary rounded-pill shadow-sm"><i class="bi bi-arrow-repeat me-1"></i> Haritadan Güncelle</button>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- ÖZET ALANLARI (COLLAPSIBLE) -->
<div class="accordion mb-4 shadow-sm" id="accordionSummaries">
    
    @if (Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding))
    {
    <!-- ESKİ BİNA (MEVCUT DURUM) -->
    <div class="accordion-item border-0 rounded-4 overflow-hidden mb-3 shadow-sm">
        <h2 class="accordion-header" id="headingOldSummary">
            <button class="accordion-button bg-danger bg-opacity-10 text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOldSummary" aria-expanded="true" aria-controls="collapseOldSummary">
                <i class="bi bi-building-dash text-danger fs-5 me-2"></i> ESKİ BİNA (Mevcut Durum) Özeti
            </button>
        </h2>
        <div id="collapseOldSummary" class="accordion-collapse collapse show" aria-labelledby="headingOldSummary">
            <div class="accordion-body p-4">
                <div class="row g-4">
                    <div class="col-md-8">
                        <h6 class="fw-bold border-bottom pb-2 mb-3">Proje Açıklaması</h6>
                        <p class="small text-muted">@Model.Description</p>

                        <div class="row mt-4">
                            <div class="col-sm-6">
                                <div class="text-muted small">Başlangıç Tarihi</div>
                                <div class="fw-bold">@Model.StartDate.ToString("dd.MM.yyyy")</div>
                            </div>
                            <div class="col-sm-6">
                                <div class="text-muted small">Bitiş Tarihi</div>
                                <div class="fw-bold">@(Model.EndDate.HasValue ? Model.EndDate.Value.ToString("dd.MM.yyyy") : "Belirtilmedi")</div>
                            </div>
                        </div>

                        <div class="mt-4 pt-3 border-top position-relative">
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Mevcut)</h6>
                                <a asp-action="Amenities" asp-route-projectId="@Model.Id" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-pencil me-1"></i> Yönet</a>
                            </div>
                            <div class="row g-3">
                                <div class="col-sm-12">
                                    <div class="d-flex align-items-center">
                                        <i class="bi bi-aspect-ratio text-success fs-4 me-3"></i>
                                        <div>
                                            <div class="text-muted small">Toplam Arazi Alanı</div>
                                            <div class="fw-bold text-dark">@(Model.TotalLandArea.HasValue ? Model.TotalLandArea.Value + " m²" : "Belirtilmedi")</div>
                                        </div>
                                    </div>
                                </div>
                                
                                @if (Model.Blocks != null && Model.Blocks.Any(b => b.IsExistingBuilding))
                                {
                                    var existingBaseArea = Model.Blocks.Where(b => b.IsExistingBuilding).Sum(b => b.BaseArea ?? 0);
                                    <div class="col-sm-6">
                                        <div class="d-flex align-items-center p-2 rounded-3 bg-danger bg-opacity-10 border border-danger border-opacity-25">
                                            <i class="bi bi-building-dash text-danger fs-4 me-3"></i>
                                            <div>
                                                <div class="text-muted small text-danger fw-bold">Eski Bina Toplam Oturumu</div>
                                                <div class="fw-bold text-dark">@(existingBaseArea > 0 ? existingBaseArea + " m²" : "Belirtilmedi")</div>
                                            </div>
                                        </div>
                                    </div>
                                }
                                
                                @if (Model.Amenities != null && Model.Amenities.Any())
                                {
                                    foreach(var amenity in Model.Amenities)
                                    {
                                        <div class="col-sm-6">
                                            <div class="d-flex align-items-center">
                                                <i class="bi bi-check-circle text-primary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small">@amenity.Name</div>
                                                    <div class="fw-bold text-dark">
                                                        @(amenity.SquareMeters.HasValue ? amenity.SquareMeters.Value + " m²" : "Mevcut")
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    }
                                }
                                else
                                {
                                    <div class="col-12 mt-2">
                                        <div class="text-muted small fst-italic"><i class="bi bi-info-circle me-1"></i>Kamelya, açık otopark, çocuk parkı gibi dış alan donatıları henüz eklenmedi.</div>
                                    </div>
                                }
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="card border-0 shadow-sm rounded-4 w-100 overflow-hidden" style="height: 250px;">
                            @if (!string.IsNullOrEmpty(Model.CurrentStateImageUrl))
                            {
                                <img src="@Model.CurrentStateImageUrl" class="img-fluid h-100 w-100" style="object-fit: cover;" alt="Proje Görseli" />
                            }
                            else
                            {
                                <div class="d-flex align-items-center justify-content-center bg-light h-100 text-muted">
                                    <div class="text-center">
                                        <i class="bi bi-image" style="font-size: 3rem;"></i>
                                        <p class="mt-2 mb-0">Görsel Yok</p>
                                    </div>
                                </div>
                            }
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    }

    @if (Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))
    {
    <!-- YENİ BİNA (HEDEF DURUM) -->
    <div class="accordion-item border-0 rounded-4 overflow-hidden shadow-sm">
        <h2 class="accordion-header" id="headingNewSummary">
            <button class="accordion-button collapsed bg-primary bg-opacity-10 text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseNewSummary" aria-expanded="false" aria-controls="collapseNewSummary">
                <i class="bi bi-building-add text-primary fs-5 me-2"></i> YENİ (HEDEF) BİNA Özeti
            </button>
        </h2>
        <div id="collapseNewSummary" class="accordion-collapse collapse" aria-labelledby="headingNewSummary">
            <div class="accordion-body p-4">
                <div class="row g-4">
                    <div class="col-md-8">
                        <div class="mt-2">
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <h6 class="fw-bold mb-0 text-muted">Arazi ve Dış Alanlar (Hedef)</h6>
                            </div>
                            <div class="row g-3">
                                <div class="col-sm-12">
                                    <div class="d-flex align-items-center">
                                        <i class="bi bi-aspect-ratio text-success fs-4 me-3"></i>
                                        <div>
                                            <div class="text-muted small">Toplam Arazi Alanı</div>
                                            <div class="fw-bold text-dark">@(Model.TotalLandArea.HasValue ? Model.TotalLandArea.Value + " m²" : "Belirtilmedi")</div>
                                        </div>
                                    </div>
                                </div>
                                
                                @if (Model.Blocks != null && Model.Blocks.Any(b => !b.IsExistingBuilding))
                                {
                                    var targetBaseArea = Model.Blocks.Where(b => !b.IsExistingBuilding).Sum(b => b.BaseArea ?? 0);
                                    <div class="col-sm-6">
                                        <div class="d-flex align-items-center p-2 rounded-3 bg-primary bg-opacity-10 border border-primary border-opacity-25">
                                            <i class="bi bi-building-add text-primary fs-4 me-3"></i>
                                            <div>
                                                <div class="text-muted small text-primary fw-bold">Yeni (Hedef) Bina Toplam Oturumu</div>
                                                <div class="fw-bold text-dark">@(targetBaseArea > 0 ? targetBaseArea + " m²" : "Belirtilmedi")</div>
                                            </div>
                                        </div>
                                    </div>
                                }
                                
                                <div class="col-12 mt-2">
                                    <div class="text-muted small fst-italic"><i class="bi bi-info-circle me-1"></i>Yeni proje için ek dış alan donatıları (Örn: Havuz, Kapalı Otopark) eklenebilir.</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="card border-0 shadow-sm rounded-4 w-100 overflow-hidden" style="height: 250px;">
                            @if (!string.IsNullOrEmpty(Model.CoverImageUrl))
                            {
                                <img src="@Model.CoverImageUrl" class="img-fluid h-100 w-100" style="object-fit: cover; filter: hue-rotate(180deg);" alt="Hedef Proje Görseli" />
                            }
                            else
                            {
                                <div class="d-flex align-items-center justify-content-center bg-light h-100 text-muted">
                                    <div class="text-center">
                                        <i class="bi bi-image" style="font-size: 3rem;"></i>
                                        <p class="mt-2 mb-0">Görsel Yok</p>
                                    </div>
                                </div>
                            }
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    }
</div>

<!-- TASLAK'''

html = pattern.sub(clean_accordion, html)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
print("Done")
