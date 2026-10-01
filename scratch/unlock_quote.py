import io

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Unlock the Teklif İste Button for Main Docs
old_btn_main = """<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste" @(isDependent && !isReadyToApply ? "disabled" : "")>"""
new_btn_main = """<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold" title="Satınalmaya Gönder / Fiyat Araştırması İste">"""
content = content.replace(old_btn_main, new_btn_main)

# 2. Add Status Badge for Main Docs
old_td_status = """else if (doc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }"""
new_td_status = """else if (doc.Status == "Fiyat Araştırılıyor") { <span class="badge bg-info text-dark rounded-pill px-3 shadow-sm fw-bold"><i class="bi bi-search"></i> Fiyat Araştırılıyor</span> }
                                                else if (doc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }"""
content = content.replace(old_td_status, new_td_status)

# 3. Add Status Badge for Child Docs
old_child_status = """@if (childDoc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }"""
new_child_status = """@if (childDoc.Status == "Fiyat Araştırılıyor") { <span class="badge bg-info text-dark rounded-pill px-3 shadow-sm fw-bold"><i class="bi bi-search"></i> Fiyat Araştırılıyor</span> }
                                                            else if (childDoc.Status == "Bekliyor") { <span class="status-badge badge-waiting">Bekliyor</span> }"""
content = content.replace(old_child_status, new_child_status)

# 4. Add Option to Modal Dropdown
old_modal_option = """<option value="İşlemde">Başvuru Yapıldı (İşlemde)</option>"""
new_modal_option = """<option value="İşlemde">Başvuru Yapıldı (İşlemde)</option>
                                    <option value="Fiyat Araştırılıyor">Fiyat Araştırılıyor (Teklif)</option>"""
content = content.replace(old_modal_option, new_modal_option)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Index.cshtml to unlock quote button and add new status.")
