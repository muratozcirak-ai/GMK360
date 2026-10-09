import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Fix the col-md-12 back to col-md-8 for both cards
content = content.replace('<div class="col-md-12">', '<div class="col-md-8">', 2)

# Fix Eski Bina image
eski_img_pattern = r'</div>\s*<div class="col-md-4">\s*<div class="card border-0 shadow-sm rounded-4 w-100 overflow-hidden" style="height: 250px;">\s*@if \(!string\.IsNullOrEmpty\(\(string\)ViewBag\.CurrentStateImageUrl\)\)\s*\{.*?\}\s*</div>\s*</div>'

eski_replacement = '''</div>
                    <div class="col-md-4 d-flex flex-column align-items-end">
                        <h6 class="fw-bold mb-3 text-muted text-end w-100" style="font-size: 0.8rem;">Mevcut Durum Görseli</h6>
                        @if (!string.IsNullOrEmpty((string)ViewBag.CurrentStateImageUrl))
                        {
                            <a href="@ViewBag.CurrentStateImageUrl" target="_blank" title="Büyük Halini Gör">
                                <div class="position-relative overflow-hidden rounded-3 border shadow-sm hover-lift" style="width: 120px; height: 120px; cursor: pointer;">
                                    <img src="@ViewBag.CurrentStateImageUrl" class="w-100 h-100" style="object-fit: cover;" alt="Proje Görseli" />
                                    <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex justify-content-center align-items-center opacity-0 hover-opacity-100 transition">
                                        <i class="bi bi-zoom-in text-white fs-4"></i>
                                    </div>
                                </div>
                            </a>
                        }
                        else
                        {
                            <div class="bg-light rounded-3 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted border" style="width: 120px; height: 120px;">
                                <i class="bi bi-camera fs-3"></i>
                            </div>
                        }
                    </div>'''
content = re.sub(eski_img_pattern, eski_replacement, content, flags=re.DOTALL)


# Fix Yeni Bina image
yeni_img_pattern = r'</div>\s*<div class="col-md-4">\s*<div class="card border-0 shadow-sm rounded-4 w-100 overflow-hidden" style="height: 250px;">\s*@if \(!string\.IsNullOrEmpty\(Model\.CoverImageUrl \?\? ViewBag\.CoverImageUrl\)\)\s*\{.*?\}\s*</div>\s*</div>'

yeni_replacement = '''</div>
                    <div class="col-md-4 d-flex flex-column align-items-end">
                        <h6 class="fw-bold mb-3 text-muted text-end w-100" style="font-size: 0.8rem;">Proje (Hedef) Görseli</h6>
                        @if (!string.IsNullOrEmpty(Model.CoverImageUrl ?? ViewBag.CoverImageUrl))
                        {
                            <a href="@(Model.CoverImageUrl ?? ViewBag.CoverImageUrl)" target="_blank" title="Büyük Halini Gör">
                                <div class="position-relative overflow-hidden rounded-3 border shadow-sm hover-lift" style="width: 120px; height: 120px; cursor: pointer;">
                                    <img src="@(Model.CoverImageUrl ?? ViewBag.CoverImageUrl)" class="w-100 h-100" style="object-fit: cover;" alt="Hedef Proje Görseli" />
                                    <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-50 d-flex justify-content-center align-items-center opacity-0 hover-opacity-100 transition">
                                        <i class="bi bi-zoom-in text-white fs-4"></i>
                                    </div>
                                </div>
                            </a>
                        }
                        else
                        {
                            <div class="bg-light rounded-3 shadow-sm d-flex flex-column align-items-center justify-content-center text-muted border" style="width: 120px; height: 120px;">
                                <i class="bi bi-camera fs-3"></i>
                            </div>
                        }
                    </div>'''
content = re.sub(yeni_img_pattern, yeni_replacement, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
