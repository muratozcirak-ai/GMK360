import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

ozet_start = '''<!-- ÖZET ALANLARI (ESKİ/YENİ BİNA KIYASLAMA) -->
<div class="row g-4 mb-4" id="accordionSummaries">'''

new_ozet_start = '''<!-- ÖZET ALANLARI (ESKİ/YENİ BİNA KIYASLAMA) -->
<div class="accordion mb-4 shadow-sm" id="accordionOzet">
    <div class="accordion-item border-0 rounded-4 overflow-hidden">
        <h2 class="accordion-header" id="headingOzet">
            <button class="accordion-button bg-light text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseOzet" aria-expanded="true" aria-controls="collapseOzet">
                <i class="bi bi-info-square-fill text-primary me-2"></i> Proje Özeti (Mevcut & Hedef Durum Kıyaslaması)
            </button>
        </h2>
        <div id="collapseOzet" class="accordion-collapse collapse show" aria-labelledby="headingOzet">
            <div class="accordion-body p-4 bg-white">
                <div class="row g-4" id="accordionSummaries">'''

content = content.replace(ozet_start, new_ozet_start)

# The end of the Ozet section is right before "<!-- TASLAK / KONU TARTIŞMA BÖLÜMÜ -->" or "<!-- STAKEHOLDERS -->"
# Let's search for what comes after the row g-4 ends.
# I know my previous script did:
# </div> <!-- END OF ROW -->
# Let's replace that.
end_of_row = '''</div> <!-- END OF ROW -->'''
new_end_of_row = '''</div> <!-- END OF ROW -->
            </div>
        </div>
    </div>
</div>'''
content = content.replace(end_of_row, new_end_of_row)


# Now for the Blocks section.
blocks_start = '''<h4 class="fw-bold mb-3"><i class="bi bi-grid-1x2 text-orange me-2"></i>Projedeki Bloklar / Binalar</h4>

@if (Model.Blocks != null && Model.Blocks.Any())
{
    var existingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding).ToList();
    var newBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding).ToList();

    if (existingBlocks.Any())
    {
        <h6 class="fw-bold text-danger border-bottom border-danger pb-2 mb-3 mt-4"><i class="bi bi-building-dash me-2"></i>Eski (Yıkılacak) Binalar</h6>
        <div class="row g-4 mb-4">
            @foreach (var block in existingBlocks)
            {
                <div class="col-md-6 col-lg-4">'''

new_blocks_start = '''<!-- PROJEDEKİ BLOKLAR (COLLAPSIBLE & 50/50 LAYOUT) -->
<div class="accordion mb-4 shadow-sm" id="accordionBloklar">
    <div class="accordion-item border-0 rounded-4 overflow-hidden">
        <h2 class="accordion-header" id="headingBloklar">
            <button class="accordion-button bg-light text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseBloklar" aria-expanded="true" aria-controls="collapseBloklar">
                <i class="bi bi-grid-1x2 text-orange me-2"></i> Projedeki Bloklar / Binalar
            </button>
        </h2>
        <div id="collapseBloklar" class="accordion-collapse collapse show" aria-labelledby="headingBloklar">
            <div class="accordion-body p-4 bg-white">
                
                @if (Model.Blocks != null && Model.Blocks.Any())
                {
                    var existingBlocks = Model.Blocks.Where(b => b.IsExistingBuilding).ToList();
                    var newBlocks = Model.Blocks.Where(b => !b.IsExistingBuilding).ToList();
                    
                    <div class="row g-4">
                        @if (existingBlocks.Any())
                        {
                            <div class="@(newBlocks.Any() ? "col-lg-6" : "col-lg-12")">
                                <h6 class="fw-bold text-danger border-bottom border-danger pb-2 mb-3"><i class="bi bi-building-dash me-2"></i>Eski (Yıkılacak) Binalar</h6>
                                <div class="row g-3">
                                    @foreach (var block in existingBlocks)
                                    {
                                        <div class="@(newBlocks.Any() ? "col-12" : "col-md-6 col-lg-4")">'''

content = content.replace(blocks_start, new_blocks_start)


# Now fix the transition between Eski Bloklar loop and Yeni Bloklar loop
blocks_middle = '''                </div>
            }
        </div>
    }

    if (newBlocks.Any())
    {
        <h6 class="fw-bold text-primary border-bottom border-primary pb-2 mb-3 mt-4"><i class="bi bi-building-add me-2"></i>Yeni (Hedef) Binalar</h6>
        <div class="row g-4 mb-4">
            @foreach (var block in newBlocks)
            {
                <div class="col-md-6 col-lg-4">'''

new_blocks_middle = '''                                        </div>
                                    }
                                </div>
                            </div>
                        }

                        @if (newBlocks.Any())
                        {
                            <div class="@(existingBlocks.Any() ? "col-lg-6" : "col-lg-12")">
                                <h6 class="fw-bold text-primary border-bottom border-primary pb-2 mb-3"><i class="bi bi-building-add me-2"></i>Yeni (Hedef) Binalar</h6>
                                <div class="row g-3">
                                    @foreach (var block in newBlocks)
                                    {
                                        <div class="@(existingBlocks.Any() ? "col-12" : "col-md-6 col-lg-4")">'''
content = content.replace(blocks_middle, new_blocks_middle)

# Now fix the end of the Blocks section
blocks_end = '''                </div>
            }
        </div>
    }
}'''

new_blocks_end = '''                                        </div>
                                    }
                                </div>
                            </div>
                        }
                    </div> <!-- End of 50/50 row -->
                }
            </div>
        </div>
    </div>
</div>'''

# We must be careful because the previous replace might have already changed the exact indentation of the end.
# Let's use regex for the end to be safe.
end_pattern = r'</div>\s*\}\s*</div>\s*\}\s*\}'
content = re.sub(end_pattern, new_blocks_end, content, count=1)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
