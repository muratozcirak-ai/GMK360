import io
import re

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Generate the new side-by-side HTML for col-md-6
new_html = """<div class="col-md-6">
                    <div class="p-3 bg-light border rounded h-100 d-flex flex-column justify-content-center">
                        <h6 class="text-muted fw-bold mb-3">Tahmini Satış ve Beklenti</h6>
                        <div class="row align-items-center">
                            <!-- Sol Taraf: Inputlar -->
                            <div class="col-lg-7 border-end">
                                <div class="input-group input-group-sm mb-2">
                                    <span class="input-group-text bg-white text-muted fw-bold" style="width: 155px; font-size: 0.8rem;">Ortalama Daire Fiyatı</span>
                                    <input type="number" id="avgFlatPrice" class="form-control text-end fw-bold text-dark" value="@(ViewBag.AvgFlatPrice == 0 ? \"\" : ((decimal)ViewBag.AvgFlatPrice).ToString(\"0.##\", System.Globalization.CultureInfo.InvariantCulture))" placeholder="0">
                                    <span class="input-group-text bg-white text-muted">₺</span>
                                </div>
                                <div class="input-group input-group-sm">
                                    <span class="input-group-text bg-white text-muted fw-bold" style="width: 155px; font-size: 0.8rem;">Toplam Dükkan Beklentisi</span>
                                    <input type="number" id="totalShopRevenue" class="form-control text-end fw-bold text-dark" value="@(ViewBag.TotalShopRevenue == 0 ? \"\" : ((decimal)ViewBag.TotalShopRevenue).ToString(\"0.##\", System.Globalization.CultureInfo.InvariantCulture))" placeholder="0">
                                    <span class="input-group-text bg-white text-muted">₺</span>
                                </div>
                            </div>
                            <!-- Sağ Taraf: Toplam Sonuç -->
                            <div class="col-lg-5 text-end">
                                <span class="text-muted small fw-bold d-block mb-1">Toplam Sonuç</span>
                                <h4 class="mb-0 fw-bold text-primary" id="estimatedTotalResult">0,00 ₺</h4>
                            </div>
                        </div>
                    </div>
                </div>"""

# Regex to find the current col-md-6 block
pattern = r'<div class="col-md-6">.*?</div>\s*</div>\s*</div>'
# Wait, parsing HTML with regex can be tricky. Let's use a more precise pattern.
pattern = r'<div class="col-md-6">\s*<div class="p-3 bg-light border rounded h-100 d-flex flex-column justify-content-between">.*?<h4 class="mb-0 fw-bold text-primary" id="estimatedTotalResult">.*?</h4>\s*</div>\s*</div>\s*</div>'

if re.search(pattern, content, re.DOTALL):
    content = re.sub(pattern, new_html, content, flags=re.DOTALL)
else:
    print("Pattern not found! Trying fallback...")
    # fallback
    pass

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated HTML to side-by-side.")
