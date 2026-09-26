import sys
import re

filepath = 'GMK360.Web/Views/ModuleDocumentRule/Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Update the HTML for the Prerequisite UI
html_old = """<label class="form-label fw-bold text-warning"><i class="ph ph-lock me-1"></i>Hangi evrak(lar) alınmadan kilit açılmasın?</label>
                                <!-- Çoklu seçim için Select2 sınıfı ekledik ve multiple yaptık -->
                                <select name="PrerequisiteTemplateIdsList" class="form-select select-multiple-native" multiple="multiple" size="4" asp-items="ViewBag.TemplatesSelect" style="width: 100%;">
                                </select>
                                <small class="text-muted mt-1 d-block">Not: Birden fazla ön koşul evrakı seçebilirsiniz. Seçilen evrakların hepsi alınmadan ana evrak kilitli kalacaktır.</small>"""

html_new = """<div class="d-flex justify-content-between align-items-center mb-2">
                                    <label class="form-label fw-bold text-warning mb-0"><i class="ph ph-lock me-1"></i>Hangi evrak(lar) alınmadan kilit açılmasın?</label>
                                    <button type="button" id="btnAddPrerequisite" class="btn btn-sm btn-warning fw-bold rounded-pill shadow-sm"><i class="ph ph-plus me-1"></i> Yeni Satır Ekle</button>
                                </div>
                                
                                <div id="prerequisiteRowsContainer">
                                    <!-- Dynamic rows will be added here -->
                                </div>
                                <small class="text-muted mt-2 d-block">Not: Eklediğiniz satırlardaki tüm evraklar şantiyede 'Alındı' olmadan, ana evrak kilitli kalacaktır.</small>
                                
                                <!-- Gizli Şablon (Yeni satır eklerken kopyalanacak) -->
                                <div id="prerequisiteTemplateRow" style="display:none;">
                                    <div class="row g-2 align-items-center mb-2 pr-row">
                                        <div class="col-10">
                                            <select name="PrerequisiteTemplateIdsList" class="form-select border-warning bg-white" asp-items="ViewBag.TemplatesSelect">
                                                <option value="">-- Havuzdan Ön Koşul Evrakı Seç --</option>
                                            </select>
                                        </div>
                                        <div class="col-2">
                                            <button type="button" class="btn btn-outline-danger btn-sm w-100 rounded-3 remove-pr" title="Satırı Sil"><i class="ph ph-trash"></i></button>
                                        </div>
                                    </div>
                                </div>"""

content = content.replace(html_old, html_new)

# 2. Update the JavaScript block
js_old = """        document.addEventListener("DOMContentLoaded", function() {
            var toggle = document.getElementById('hasPrerequisiteToggle');
            var div = document.getElementById('prerequisiteDiv');
            var selectMulti = document.querySelector('.select-multiple-native');

            if(toggle) {
                toggle.addEventListener('change', function() {
                    if(this.checked) {
                        div.style.display = 'block';
                    } else {
                        div.style.display = 'none';
                        if(selectMulti) {
                            for(var i=0; i<selectMulti.options.length; i++){
                                selectMulti.options[i].selected = false;
                            }
                        }
                    }
                });
            }
        });"""

js_new = """        document.addEventListener("DOMContentLoaded", function() {
            var toggle = document.getElementById('hasPrerequisiteToggle');
            var div = document.getElementById('prerequisiteDiv');
            var container = document.getElementById('prerequisiteRowsContainer');
            var template = document.getElementById('prerequisiteTemplateRow').innerHTML;
            var btnAdd = document.getElementById('btnAddPrerequisite');

            if(toggle) {
                toggle.addEventListener('change', function() {
                    if(this.checked) {
                        div.style.display = 'block';
                        // İlk açıldığında boşsa 1 tane otomatik ekle
                        if(container.children.length === 0) {
                            container.insertAdjacentHTML('beforeend', template);
                        }
                    } else {
                        div.style.display = 'none';
                        container.innerHTML = ''; // Temizle
                    }
                });
            }

            if(btnAdd) {
                btnAdd.addEventListener('click', function() {
                    container.insertAdjacentHTML('beforeend', template);
                });
            }

            // Dinamik silme butonları için event delegation
            if(container) {
                container.addEventListener('click', function(e) {
                    if(e.target.closest('.remove-pr')) {
                        var row = e.target.closest('.pr-row');
                        if(row) {
                            row.remove();
                        }
                    }
                });
            }
        });"""

content = content.replace(js_old, js_new)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
