import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Locate start
start_marker = '<div class="wizard-step active" id="step1">'
end_marker = '<div class="col-md-12">\n                                <label class="form-label fw-bold">Kısa Açıklama'

start_idx = content.find(start_marker)
end_idx = content.find(end_marker)

if start_idx != -1 and end_idx != -1:
    new_section = '''<div class="wizard-step active" id="step1">
                        <h4 class="fw-bold text-navy mb-4"><i class="ph ph-info text-orange me-2"></i> Proje Künyesi</h4>
                        <div class="row g-4 mb-3">
                            
                            <!-- 1. Proje Adı -->
                            <div class="col-md-6">
                                <label class="form-label fw-bold text-primary">Proje Adı <span class="text-danger">*</span></label>
                                <input asp-for="Name" class="form-control form-control-lg rounded-3 border-primary" placeholder="Örn: Vadi Konakları" required />
                            </div>
                            
                            <!-- 2. Proje Durumu -->
                            <div class="col-md-6">
                                <label class="form-label fw-bold">Proje Durumu <span class="text-danger">*</span></label>
                                <select asp-for="StatusId" class="form-select form-select-lg rounded-3 border-secondary" required>
                                    <option value="2">Devam Eden Proje (Aktif Şantiye)</option>
                                    <option value="0">Aday Proje / Fizibilite (Kentsel Dönüşüm)</option>
                                    <option value="1">Aday Proje (Teklif)</option>
                                    <option value="3">Tamamlandı / Teslim</option>
                                </select>
                            </div>

                            <!-- 3. Geliştirme Yöntemi (Kentsel Dönüşüm vb.) -->
                            <div class="col-md-6">
                                <label class="form-label fw-bold">Geliştirme Yöntemi <span class="text-danger">*</span></label>
                                <select asp-for="ProjectOriginId" class="form-select form-select-lg rounded-3 border-danger" required>
                                    <option value="1">Kentsel Dönüşüm (Yıkılacak Eski Yapı Var)</option>
                                    <option value="0">Sıfırdan İnşaat (Boş Arazi)</option>
                                    <option value="2">Kat Karşılığı</option>
                                </select>
                            </div>

                            <!-- 4. Yapı Türü -->
                            <div class="col-md-6">
                                <label class="form-label fw-bold">Yapı Türü <span class="text-danger">*</span></label>
                                <select asp-for="ProjectType" class="form-select form-select-lg rounded-3 border-info" required>
                                    <option value="">Lütfen Projenin Türünü Seçiniz...</option>
                                    <option value="Apartman">Apartman</option>
                                    <option value="Site">Site / Kompleks</option>
                                    <option value="Ticari">Ticari Plaza / İş Merkezi</option>
                                    <option value="Karma">Karma Proje (Konut + Ticari)</option>
                                </select>
                            </div>
                        </div>
                        <div class="row g-4">
                            ''' 
    content = content[:start_idx] + new_section + content[end_idx:]
    
    with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
        f.write(content)
    print("Fixed!")
else:
    print(f"Indices: start={start_idx}, end={end_idx}")

