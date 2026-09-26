import sys

filepath = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Table Headers
target_th = """<th>Bitiş T.</th>
                                                            <th>Durum</th>"""
replacement_th = """<th>Bitiş T.</th>
                                                            <th>Maliyet</th>
                                                            <th>Durum</th>"""
content = content.replace(target_th, replacement_th)

# 2. Main Doc Row
target_main_td = """<td>@(doc.CompletedDate.HasValue ? doc.CompletedDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                <td>"""
replacement_main_td = """<td>@(doc.CompletedDate.HasValue ? doc.CompletedDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                <td title="Harç/Belge: @(doc.DocumentFee?.ToString("N2") ?? "0,00") ₺ | Ek Masraf: @(doc.AdditionalCost?.ToString("N2") ?? "0,00") ₺"><strong>@((doc.DocumentFee.GetValueOrDefault() + doc.AdditionalCost.GetValueOrDefault()).ToString("N2")) ₺</strong></td>
                                                                <td>"""
content = content.replace(target_main_td, replacement_main_td)

# 3. Child Doc Row
target_child_td = """<td>@(childDoc.CompletedDate.HasValue ? childDoc.CompletedDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                            <td>"""
replacement_child_td = """<td>@(childDoc.CompletedDate.HasValue ? childDoc.CompletedDate.Value.ToString("dd.MM.yyyy") : "-")</td>
                                                                            <td title="Harç/Belge: @(childDoc.DocumentFee?.ToString("N2") ?? "0,00") ₺ | Ek Masraf: @(childDoc.AdditionalCost?.ToString("N2") ?? "0,00") ₺"><strong>@((childDoc.DocumentFee.GetValueOrDefault() + childDoc.AdditionalCost.GetValueOrDefault()).ToString("N2")) ₺</strong></td>
                                                                            <td>"""
content = content.replace(target_child_td, replacement_child_td)

# 4. Modal HTML
target_modal_html = """<div class="col-md-12">
                            <label class="form-label fw-semibold">Açıklama / Sorun Notu</label>"""
replacement_modal_html = """<div class="col-md-6">
                            <label class="form-label fw-semibold">Resmi Harç / Fatura Bedeli (₺)</label>
                            <input type="number" step="0.01" class="form-control" id="modalDocumentFee" name="DocumentFee" placeholder="0.00">
                        </div>
                        <div class="col-md-6">
                            <label class="form-label fw-semibold">Ek Masraf (Yol, Yemek, Kargo) (₺)</label>
                            <input type="number" step="0.01" class="form-control" id="modalAdditionalCost" name="AdditionalCost" placeholder="0.00">
                        </div>
                        
                        <div class="col-md-12">
                            <label class="form-label fw-semibold">Açıklama / Sorun Notu</label>"""
if "modalDocumentFee" not in content:
    content = content.replace(target_modal_html, replacement_modal_html)

# 5. Modal JS Fetch
target_modal_js = """document.getElementById('modalCompletedDate').value = data.completedDate ? data.completedDate.split('T')[0] : '';
                document.getElementById('modalIssueNotes').value = data.issueNotes || '';"""
replacement_modal_js = """document.getElementById('modalCompletedDate').value = data.completedDate ? data.completedDate.split('T')[0] : '';
                document.getElementById('modalDocumentFee').value = data.documentFee || '';
                document.getElementById('modalAdditionalCost').value = data.additionalCost || '';
                document.getElementById('modalIssueNotes').value = data.issueNotes || '';"""
if "document.getElementById('modalDocumentFee')" not in content:
    content = content.replace(target_modal_js, replacement_modal_js)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
    print("View updated for costs.")
