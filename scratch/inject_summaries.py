import io

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
for i, line in enumerate(lines):
    new_lines.append(line)
    
    # Injection for Old Building
    if '@if (Model.Amenities != null && Model.Amenities.Any())' in line and 'foreach' not in lines[i+1]:
        # Inject right before amenities in old building block
        injection = """                                @{
                                    int eskiToplamBlok = eskiBloklar?.Count ?? 0;
                                    int eskiToplamDaire = eskiBloklar?.Sum(b => b.TotalApartments) ?? 0;
                                    int eskiToplamDukkan = eskiBloklar?.Sum(b => b.TotalShops) ?? 0;
                                    int eskiMaxKat = (eskiBloklar != null && eskiBloklar.Any()) ? eskiBloklar.Max(b => (b.TotalFloors ?? 0) + (b.BasementFloors ?? 0)) : 0;
                                    string eskiNizam = (eskiBloklar != null && eskiBloklar.Any()) ? string.Join(", ", eskiBloklar.Where(b => !string.IsNullOrEmpty(b.LayoutPattern)).Select(b => b.LayoutPattern).Distinct()) : "Belirtilmedi";
                                    if (string.IsNullOrEmpty(eskiNizam)) eskiNizam = "Belirtilmedi";
                                }
                                <div class="col-12 mt-1 mb-2">
                                    <div class="d-flex flex-wrap gap-2">
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-grid-3x3-gap me-1 text-muted"></i> @eskiNizam</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-building me-1 text-muted"></i> @eskiToplamBlok Blok</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-door-closed me-1 text-muted"></i> @eskiToplamDaire Daire</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-shop me-1 text-muted"></i> @eskiToplamDukkan Dükkan</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-layers me-1 text-muted"></i> Max @eskiMaxKat Kat</span>
                                    </div>
                                </div>\n"""
        new_lines.insert(-1, injection)

    # Injection for New Building
    if '<div class="col-12 mt-2">' in line and 'Yeni proje' in lines[i+1]:
        injection2 = """                                @{
                                    var yeniBloklar = Model.Blocks?.Where(b => !b.IsExistingBuilding).ToList();
                                    int yeniToplamBlok = yeniBloklar?.Count ?? 0;
                                    int yeniToplamDaire = yeniBloklar?.Sum(b => b.TotalApartments) ?? 0;
                                    int yeniToplamDukkan = yeniBloklar?.Sum(b => b.TotalShops) ?? 0;
                                    int yeniMaxKat = (yeniBloklar != null && yeniBloklar.Any()) ? yeniBloklar.Max(b => (b.TotalFloors ?? 0) + (b.BasementFloors ?? 0)) : 0;
                                    string yeniNizam = (yeniBloklar != null && yeniBloklar.Any()) ? string.Join(", ", yeniBloklar.Where(b => !string.IsNullOrEmpty(b.LayoutPattern)).Select(b => b.LayoutPattern).Distinct()) : "Belirtilmedi";
                                    if (string.IsNullOrEmpty(yeniNizam)) yeniNizam = "Belirtilmedi";
                                }
                                <div class="col-12 mt-1 mb-2">
                                    <div class="d-flex flex-wrap gap-2">
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-grid-3x3-gap me-1 text-muted"></i> @yeniNizam</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-building me-1 text-muted"></i> @yeniToplamBlok Blok</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-door-closed me-1 text-muted"></i> @yeniToplamDaire Daire</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-shop me-1 text-muted"></i> @yeniToplamDukkan Dükkan</span>
                                        <span class="badge bg-light text-dark border p-2"><i class="bi bi-layers me-1 text-muted"></i> Max @yeniMaxKat Kat</span>
                                    </div>
                                </div>\n"""
        new_lines.insert(-1, injection2)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print("Injected summaries")
