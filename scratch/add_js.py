import io
import re

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix the value="" attributes
content = content.replace('id="avgFlatPrice" class="form-control text-end fw-bold text-dark" placeholder="0"', 'id="avgFlatPrice" class="form-control text-end fw-bold text-dark" value="@(ViewBag.AvgFlatPrice == 0 ? \"\" : ViewBag.AvgFlatPrice)" placeholder="0"')
content = content.replace('id="totalShopRevenue" class="form-control text-end fw-bold text-dark" placeholder="0"', 'id="totalShopRevenue" class="form-control text-end fw-bold text-dark" value="@(ViewBag.TotalShopRevenue == 0 ? \"\" : ViewBag.TotalShopRevenue)" placeholder="0"')

# Add the JavaScript to the bottom
js_block = """
@section Scripts {
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const avgFlatPriceInput = document.getElementById('avgFlatPrice');
            const totalShopRevenueInput = document.getElementById('totalShopRevenue');
            const estimatedTotalResult = document.getElementById('estimatedTotalResult');
            
            // ViewBag'den gelen kalan daire sayisi (string olarak sayfa html'ine gomuyoruz)
            const leftFlatsCount = parseInt('@ViewBag.LeftFlats') || 0;
            const projectId = parseInt('@ViewData["ProjectId"]');

            function calculateAndRender() {
                const avgPrice = parseFloat(avgFlatPriceInput.value) || 0;
                const shopRev = parseFloat(totalShopRevenueInput.value) || 0;
                
                const total = (leftFlatsCount * avgPrice) + shopRev;
                
                // Format decimal to TR locale (e.g. 1.500.000,00)
                estimatedTotalResult.textContent = total.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' ₺';
            }

            function saveToDatabase() {
                const avgPrice = parseFloat(avgFlatPriceInput.value) || 0;
                const shopRev = parseFloat(totalShopRevenueInput.value) || 0;

                fetch('/ProjectFinance/UpdateExpectations', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        projectId: projectId,
                        avgFlatPrice: avgPrice,
                        totalShopRevenue: shopRev
                    })
                }).then(res => {
                    if(!res.ok) console.error("Kayıt sırasında hata oluştu.");
                }).catch(err => console.error(err));
            }

            // Anında hesapla
            avgFlatPriceInput.addEventListener('input', calculateAndRender);
            totalShopRevenueInput.addEventListener('input', calculateAndRender);

            // Odak dışına çıkınca (blur) veya Enter'a basınca otomatik kaydet (buton olmadan)
            avgFlatPriceInput.addEventListener('change', saveToDatabase);
            totalShopRevenueInput.addEventListener('change', saveToDatabase);
        });
    </script>
}
"""

if "@section Scripts" not in content:
    content += "\n" + js_block
else:
    # Just to be safe, replace the end of the file if section Scripts exists but this is probably not the case
    pass

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("HTML and JS updated.")
