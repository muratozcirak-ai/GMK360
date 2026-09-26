import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern for old block injection
old_pattern = r'(@\{\s*int eskiToplamBlok = eskiBloklar\?\.Count \?\? 0;.*?</div>\s*</div>)'
new_old_html = r"""@{
                                    int eskiToplamBlok = eskiBloklar?.Count ?? 0;
                                    int eskiToplamDaire = eskiBloklar?.Sum(b => b.TotalApartments) ?? 0;
                                    int eskiToplamDukkan = eskiBloklar?.Sum(b => b.TotalShops) ?? 0;
                                    int eskiMaxKat = (eskiBloklar != null && eskiBloklar.Any()) ? eskiBloklar.Max(b => (b.TotalFloors ?? 0) + (b.BasementFloors)) : 0;
                                    string eskiNizam = (eskiBloklar != null && eskiBloklar.Any()) ? string.Join(", ", eskiBloklar.Where(b => !string.IsNullOrEmpty(b.LayoutPattern)).Select(b => b.LayoutPattern).Distinct()) : "Belirtilmedi";
                                    if (string.IsNullOrEmpty(eskiNizam)) eskiNizam = "Belirtilmedi";
                                }
                                <div class="col-sm-12 mt-2">
                                    <div class="row g-2">
                                        <div class="col-md-4">
                                            <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                <i class="bi bi-grid-3x3-gap text-secondary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small fw-bold">Nizam & Blok</div>
                                                    <div class="fw-bold text-dark">@eskiNizam, @eskiToplamBlok Blok</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                <i class="bi bi-door-open text-secondary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small fw-bold">Bağımsız Bölüm</div>
                                                    <div class="fw-bold text-dark">@eskiToplamDaire Daire, @eskiToplamDukkan Dükkan</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                <i class="bi bi-layers text-secondary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small fw-bold">Kat Sayısı</div>
                                                    <div class="fw-bold text-dark">Bodrum Dahil Max @eskiMaxKat Kat</div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>"""
content = re.sub(old_pattern, new_old_html, content, flags=re.DOTALL)

# Pattern for new block injection
new_pattern = r'(@\{\s*var yeniBloklar = Model\.Blocks\?\.Where.*?</div>\s*</div>)'
new_new_html = r"""@{
                                    var yeniBloklar = Model.Blocks?.Where(b => !b.IsExistingBuilding).ToList();
                                    int yeniToplamBlok = yeniBloklar?.Count ?? 0;
                                    int yeniToplamDaire = yeniBloklar?.Sum(b => b.TotalApartments) ?? 0;
                                    int yeniToplamDukkan = yeniBloklar?.Sum(b => b.TotalShops) ?? 0;
                                    int yeniMaxKat = (yeniBloklar != null && yeniBloklar.Any()) ? yeniBloklar.Max(b => (b.TotalFloors ?? 0) + (b.BasementFloors)) : 0;
                                    string yeniNizam = (yeniBloklar != null && yeniBloklar.Any()) ? string.Join(", ", yeniBloklar.Where(b => !string.IsNullOrEmpty(b.LayoutPattern)).Select(b => b.LayoutPattern).Distinct()) : "Belirtilmedi";
                                    if (string.IsNullOrEmpty(yeniNizam)) yeniNizam = "Belirtilmedi";
                                }
                                <div class="col-sm-12 mt-2">
                                    <div class="row g-2">
                                        <div class="col-md-4">
                                            <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                <i class="bi bi-grid-3x3-gap text-secondary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small fw-bold">Nizam & Blok</div>
                                                    <div class="fw-bold text-dark">@yeniNizam, @yeniToplamBlok Blok</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                <i class="bi bi-door-open text-secondary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small fw-bold">Bağımsız Bölüm</div>
                                                    <div class="fw-bold text-dark">@yeniToplamDaire Daire, @yeniToplamDukkan Dükkan</div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="d-flex align-items-center p-2 rounded-3 bg-light border">
                                                <i class="bi bi-layers text-secondary fs-4 me-3"></i>
                                                <div>
                                                    <div class="text-muted small fw-bold">Kat Sayısı</div>
                                                    <div class="fw-bold text-dark">Bodrum Dahil Max @yeniMaxKat Kat</div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>"""
content = re.sub(new_pattern, new_new_html, content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated to use standard boxes instead of badges.")
