import io
import re

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# We want to replace the last col-md-4 block before the </row> which is right before "Genel Özet Tablosu"
# Let's use regex to find it.

pattern = r'(<div class="col-md-4">\s*<div class="p-3 bg-light border rounded">\s*<h6 class="text-muted fw-bold mb-1">Tahmini Sat.*?</h6>\s*<h4 class="mb-0 fw-bold text-primary">@ViewBag\.EstimatedValue</h4>\s*</div>\s*</div>)'

new_html = """<div class="col-md-4">
                    <div class="p-3 bg-light border rounded h-100 d-flex flex-column justify-content-between">
                        <div>
                            <h6 class="text-muted fw-bold mb-3">Tahmini Satış ve Beklenti</h6>
                            
                            <!-- 1. Ortalama Daire Fiyatı -->
                            <div class="input-group input-group-sm mb-2">
                                <span class="input-group-text bg-white text-muted fw-bold" style="width: 155px; font-size: 0.8rem;">Ortalama Daire Fiyatı</span>
                                <input type="number" id="avgFlatPrice" class="form-control text-end fw-bold text-dark" placeholder="0">
                                <span class="input-group-text bg-white text-muted">₺</span>
                            </div>
                            
                            <!-- 2. Toplam Dükkan Beklentisi -->
                            <div class="input-group input-group-sm mb-3">
                                <span class="input-group-text bg-white text-muted fw-bold" style="width: 155px; font-size: 0.8rem;">Toplam Dükkan Beklentisi</span>
                                <input type="number" id="totalShopRevenue" class="form-control text-end fw-bold text-dark" placeholder="0">
                                <span class="input-group-text bg-white text-muted">₺</span>
                            </div>
                        </div>

                        <!-- Dinamik Sonuç Alanı -->
                        <div class="text-end pt-2 border-top mt-auto">
                            <span class="text-muted small fw-bold d-block mb-1">Toplam Sonuç</span>
                            <h4 class="mb-0 fw-bold text-primary" id="estimatedTotalResult">0,00 ₺</h4>
                        </div>
                    </div>
                </div>"""

# Replace
if re.search(pattern, content, re.DOTALL):
    content = re.sub(pattern, new_html, content, flags=re.DOTALL)
    with io.open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Success")
else:
    print("Pattern not found!")

