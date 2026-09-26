import re

filepath = r'GMK360.Web\Views\ConstructionProject\Create.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r'<div class="col-md-12 mb-2">\s*<label class="form-label fw-bold">Proje Durumu / Tipi</label>\s*<select asp-for="StatusId" style="pointer-events: none; background-color: #e9ecef;" tabindex="-1" readonly class="form-select form-select-lg rounded-3 border-primary" onchange="handleStatusChange\(\)">.*?<div class="form-text text-muted" id="statusHelpText">.*?</div>\s*</div>'

replacement = """<div class="col-md-12 mb-2">
                                <label class="form-label fw-bold">Proje Durumu / Tipi</label>
                                @{
                                    string statusText = Model.StatusId switch {
                                        0 => "Ön Görüşme / Fizibilite",
                                        1 => "Aday Proje (Teklif)",
                                        2 => "Aktif Şantiye (Devam Eden)",
                                        3 => "Tamamlandı / Teslim",
                                        4 => "Satışta (Topraktan)",
                                        5 => "Anlaşma Yapıldı",
                                        6 => "İptal / Anlaşma Olmadı",
                                        _ => "Belirsiz"
                                    };
                                }
                                <div class="form-control form-control-lg rounded-3 border-primary" style="background-color: #e9ecef;">
                                    <i class="ph ph-info me-2 text-primary"></i> <strong>@statusText</strong>
                                </div>
                                <input type="hidden" asp-for="StatusId" />
                                <div class="form-text text-muted">Projenizin mevcut statüsü. Statü değişikliklerini projenin ana panelinden yapabilirsiniz.</div>
                            </div>"""

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Create.cshtml updated.")
