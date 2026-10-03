import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Update Table Cell for Cost
target_cell = r'<td class="text-end pe-4 fw-bold text-dark">@item\.PlannedUnitPrice\.ToString\("N2"\) ₺</td>'
replacement_cell = '''<td class="text-end pe-4">
                                                    <div class="fw-bold text-dark fs-6">@item.PlannedTotalCost.ToString("N2") ₺</div>
                                                    @if (item.Quantity != 1 || item.Unit != "Götürü")
                                                    {
                                                        <small class="text-muted" style="font-size:0.75rem;">@item.Quantity.ToString("G29") @item.Unit x @item.PlannedUnitPrice.ToString("N2") ₺</small>
                                                    }
                                                    else
                                                    {
                                                        <small class="text-muted" style="font-size:0.75rem;">Götürü Bedel</small>
                                                    }
                                                </td>'''
content = re.sub(target_cell, replacement_cell, content)

# Update openManageModal button calls
target_btn1 = r'onclick="openManageModal\(@item\.Id, \'@item\.ItemName \(Aylık Kira\)\', \'@item\.PlannedUnitPrice\.ToString\(System\.Globalization\.CultureInfo\.InvariantCulture\)\', \'@item\.Description\'\)"'
replacement_btn1 = '''onclick="openManageModal(@item.Id, '@item.ItemName (Kira)', '@item.PlannedTotalCost.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description', '@item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Unit')"'''
content = re.sub(target_btn1, replacement_btn1, content)

target_btn2 = r'onclick="openManageModal\(@item\.Id, \'@item\.ItemName \(Nakliye/Transfer Bedeli\)\', \'@item\.PlannedUnitPrice\.ToString\(System\.Globalization\.CultureInfo\.InvariantCulture\)\', \'@item\.Description\'\)"'
replacement_btn2 = '''onclick="openManageModal(@item.Id, '@item.ItemName (Nakliye/Transfer Bedeli)', '@item.PlannedTotalCost.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description', '1', 'Götürü')"'''
content = re.sub(target_btn2, replacement_btn2, content)

target_btn3 = r'onclick="openManageModal\(@item\.Id, \'@item\.ItemName\', \'@item\.PlannedUnitPrice\.ToString\(System\.Globalization\.CultureInfo\.InvariantCulture\)\', \'@item\.Description\'\)"'
replacement_btn3 = '''onclick="openManageModal(@item.Id, '@item.ItemName', '@item.PlannedTotalCost.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Description', '@item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)', '@item.Unit')"'''
content = re.sub(target_btn3, replacement_btn3, content)

# Update Manage Modal HTML
target_manage_modal = r'<div class="modal-body">\s*<div class="mb-3">\s*<label class="form-label fw-bold text-dark">Tahmini / Sabit Tutar \(₺\)</label>\s*<div class="input-group">\s*<input type="number" step="0\.01" name="plannedUnitPrice" id="managePrice" class="form-control" required />\s*<span class="input-group-text">₺</span>\s*</div>\s*</div>\s*<div class="mb-3">\s*<label class="form-label fw-bold text-dark">Açıklama / Notlar</label>\s*<textarea name="description" id="manageDesc" class="form-control" rows="2"></textarea>\s*</div>\s*</div>'
replacement_manage_modal = '''<div class="modal-body">
                <div class="row g-3 mb-3">
                    <div class="col-md-6">
                        <label class="form-label fw-bold text-dark">Miktar (Opsiyonel)</label>
                        <input type="number" step="0.01" name="quantity" id="manageQty" class="form-control" value="1" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label fw-bold text-dark">Birim</label>
                        <select name="unit" id="manageUnit" class="form-select">
                            <option value="Götürü">Götürü</option>
                            <option value="m2">m2</option>
                            <option value="m3">m3</option>
                            <option value="Ton">Ton</option>
                            <option value="Kg">Kg</option>
                            <option value="Adet">Adet</option>
                            <option value="Ay">Ay</option>
                            <option value="Gün">Gün</option>
                            <option value="Saat">Saat</option>
                            <option value="Sefer">Sefer</option>
                            <option value="Paket">Paket</option>
                        </select>
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-primary">Tahmini Toplam Tutar (₺)</label>
                    <div class="input-group">
                        <input type="number" step="0.01" name="totalCost" id="managePrice" class="form-control border-primary" required />
                        <span class="input-group-text bg-primary text-white border-primary">₺</span>
                    </div>
                    <small class="text-muted mt-1 d-block"><i class="bi bi-info-circle"></i> Miktar girmezseniz sistem Götürü (Lump Sum) olarak hesaplar.</small>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-dark">Açıklama / Notlar</label>
                    <textarea name="description" id="manageDesc" class="form-control" rows="2"></textarea>
                </div>
            </div>'''
content = re.sub(target_manage_modal, replacement_manage_modal, content)

# Update Add Modal HTML
target_add_modal = r'<div class="mb-3">\s*<label class="form-label fw-bold text-dark">Tahmini Tutar \(Opsiyonel\)</label>\s*<div class="input-group">\s*<input type="number" step="0\.01" name="plannedUnitPrice" class="form-control" value="0" />\s*<span class="input-group-text">₺</span>\s*</div>\s*<small class="text-muted mt-1 d-block">Tutarı sonra girmek için 0 bırakabilirsiniz\.</small>\s*</div>'
replacement_add_modal = '''<div class="row g-3 mb-3">
                    <div class="col-md-6">
                        <label class="form-label fw-bold text-dark">Miktar (Opsiyonel)</label>
                        <input type="number" step="0.01" name="quantity" class="form-control" value="1" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label fw-bold text-dark">Birim</label>
                        <select name="unit" class="form-select">
                            <option value="Götürü">Götürü</option>
                            <option value="m2">m2</option>
                            <option value="m3">m3</option>
                            <option value="Ton">Ton</option>
                            <option value="Kg">Kg</option>
                            <option value="Adet">Adet</option>
                            <option value="Ay">Ay</option>
                            <option value="Gün">Gün</option>
                            <option value="Saat">Saat</option>
                            <option value="Sefer">Sefer</option>
                            <option value="Paket">Paket</option>
                        </select>
                    </div>
                </div>
                <div class="mb-3">
                    <label class="form-label fw-bold text-primary">Tahmini Toplam Tutar (Opsiyonel)</label>
                    <div class="input-group">
                        <input type="number" step="0.01" name="totalCost" class="form-control border-primary" value="0" />
                        <span class="input-group-text bg-primary text-white border-primary">₺</span>
                    </div>
                    <small class="text-muted mt-1 d-block">Tutarı sonra girmek için 0 bırakabilirsiniz.</small>
                </div>'''
content = re.sub(target_add_modal, replacement_add_modal, content)

# Update openManageModal JS
target_js = r'function openManageModal\(id, title, price, desc\) \{\s*document\.getElementById\(\'manageId\'\)\.value = id;\s*document\.getElementById\(\'manageTitle\'\)\.innerText = title;\s*document\.getElementById\(\'managePrice\'\)\.value = price;\s*document\.getElementById\(\'manageDesc\'\)\.value = desc !== \'null\' \? desc : \'\';'
replacement_js = '''function openManageModal(id, title, price, desc, qty, unit) {
        document.getElementById('manageId').value = id;
        document.getElementById('manageTitle').innerText = title;
        document.getElementById('managePrice').value = price;
        document.getElementById('manageDesc').value = desc !== 'null' ? desc : '';
        document.getElementById('manageQty').value = qty !== 'null' ? qty : '1';
        document.getElementById('manageUnit').value = unit !== 'null' && unit !== '' ? unit : 'Götürü';'''
content = re.sub(target_js, replacement_js, content)

# Update the Info text!
target_info = r'Şantiyenizin özel fiziki şartlarına \(örn: dağ başında olduğu için su tankeri gerekmesi\) veya şirketinizin demirbaş durumuna \(örn: JCB veya Konteyneriniz yoksa kiralama gerekmesi\) göre doğacak ek ihtiyaçları, ilgili kategorinin altındaki <strong>"Yeni Bütçe Kalemi Ekle"</strong> butonunu kullanarak fizibilitenize eklemelisiniz\.</p>'
replacement_info = r'Şantiyenizin özel fiziki şartlarına veya şirketinizin demirbaş durumuna göre doğacak ek ihtiyaçları, alt kısımdaki <strong>"Yeni Bütçe Kalemi Ekle"</strong> butonunu kullanarak fizibilitenize eklemelisiniz.</p><hr class="my-2 border-info opacity-50"><p class="mb-0 small text-dark"><i class="bi bi-lightning-charge-fill text-warning"></i> <strong>Patron ve Usta Esnekliği (Götürü Bedel):</strong> Sistem sizi milimetrik metraj girmeye zorlamaz. Eğer çizimler bitmemişse veya tecrübenize dayanarak kaba bir bütçe ayırıyorsanız, "Miktar ve Birim" kısımlarını pas geçip doğrudan <strong>Tahmini Toplam Tutar</strong> hücresine elden rakam girebilirsiniz. Böylece program veri girişi sıkıcılığından çıkar, usta şantiyecilerin kullandığı hızlı bir öngörü silahına dönüşür.</p>'
content = re.sub(target_info, replacement_info, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)