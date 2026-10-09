with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

import re

new_js = r"""        function prepareSummary() {
            const name = document.querySelector('input[name="Name"]').value;
            
            // Container elements for rendering
            const oldContainer = document.getElementById('oldSummaryCard');
            const newContainer = document.getElementById('newSummaryCard');
            
            function buildBlockSummary(blockItem, isOld) {
                const title = blockItem.querySelector('input[type="text"]').value || (isOld ? "Mevcut Yapı" : "Yeni Yapı");
                const layoutSelect = blockItem.querySelector('select[name$=".LayoutPattern"]');
                const layout = layoutSelect ? layoutSelect.value : "";
                
                let html = '';
                let totalApts = 0;
                let totalShops = 0;
                let totalSubBlocks = 0;
                
                // Read subblocks
                const subBlocks = blockItem.querySelectorAll('.sub-blocks-list .card');
                let subBlocksHtml = '';
                
                if (subBlocks.length > 0) {
                    totalSubBlocks = subBlocks.length;
                    subBlocks.forEach((sb, idx) => {
                        const sbName = sb.querySelector('input[name$=".BlockName"]').value || `${idx+1}. Alt Yapı`;
                        const sbApts = parseInt(sb.querySelector('input[name$=".TotalApartments"]').value) || 0;
                        const sbShops = parseInt(sb.querySelector('input[name$=".TotalShops"]').value) || 0;
                        const sbFloors = parseInt(sb.querySelector('input[name$=".TotalFloors"]').value) || 0;
                        
                        totalApts += sbApts;
                        totalShops += sbShops;
                        
                        subBlocksHtml += `
                            <div class="col-6 col-md-3 mb-2">
                                <div class="border rounded-3 p-2 bg-white text-center shadow-sm">
                                    <div class="fw-bold small text-dark mb-1">${sbName}</div>
                                    <div class="small text-muted">${sbFloors} Kat | ${sbApts} D, ${sbShops} Dük.</div>
                                </div>
                            </div>
                        `;
                    });
                } else {
                    // No subblocks, read from main block
                    totalApts = parseInt(blockItem.querySelector('input[name$=".TotalApartments"]').value) || 0;
                    totalShops = parseInt(blockItem.querySelector('input[name$=".TotalShops"]').value) || 0;
                    totalSubBlocks = 1;
                }
                
                // Visual Theme based on Layout
                const themeColor = isOld ? "danger" : "primary";
                const badgeColor = isOld ? "bg-danger" : "bg-primary";
                
                let visualTitle = "";
                if (layout.includes("Ortak Baza")) {
                    visualTitle = `<span class="badge ${badgeColor}"><i class="ph ph-intersect"></i> Ortak Baza (${totalSubBlocks} Kule)</span>`;
                } else if (layout.includes("Biti")) {
                    visualTitle = `<span class="badge ${badgeColor}"><i class="ph ph-buildings"></i> Bitişik Nizam (${totalSubBlocks} Blok)</span>`;
                } else {
                    visualTitle = `<span class="badge ${badgeColor}"><i class="ph ph-building"></i> Tekil Yapı</span>`;
                }
                
                html += `
                <div class="card bg-white border-${themeColor} border-opacity-25 shadow-sm rounded-4 mb-3">
                    <div class="card-header bg-${themeColor} bg-opacity-10 border-0 d-flex justify-content-between align-items-center">
                        <h6 class="fw-bold text-${themeColor} mb-0">${title}</h6>
                        ${visualTitle}
                    </div>
                    <div class="card-body p-3">
                        <div class="row text-center mb-3 g-2">
                            <div class="col-4 border-end border-${themeColor} border-opacity-25">
                                <div class="fs-4 fw-bold text-dark">${totalSubBlocks}</div>
                                <div class="small text-muted text-uppercase tracking-wide">Yapı</div>
                            </div>
                            <div class="col-4 border-end border-${themeColor} border-opacity-25">
                                <div class="fs-4 fw-bold text-dark">${totalApts}</div>
                                <div class="small text-muted text-uppercase tracking-wide">Daire</div>
                            </div>
                            <div class="col-4">
                                <div class="fs-4 fw-bold text-dark">${totalShops}</div>
                                <div class="small text-muted text-uppercase tracking-wide">Dükkan</div>
                            </div>
                        </div>
                        ${subBlocksHtml ? `<div class="p-3 bg-light rounded-3 border"><div class="row g-2">${subBlocksHtml}</div></div>` : ''}
                    </div>
                </div>
                `;
                
                return { html, totalApts, totalShops };
            }
            
            // Build OLD
            let oldHtml = `<h5 class="fw-bold text-danger mb-3"><i class="ph ph-buildings me-2"></i> ${name} (Mevcut Yapılar)</h5>`;
            let gOldApts = 0, gOldShops = 0;
            const oldBlocks = document.querySelectorAll('.block-item-old');
            oldBlocks.forEach(b => {
                const res = buildBlockSummary(b, true);
                oldHtml += res.html;
                gOldApts += res.totalApts;
                gOldShops += res.totalShops;
            });
            oldContainer.innerHTML = oldHtml;
            oldContainer.className = "mb-4"; // reset classes so it looks clean
            
            // Build NEW
            let newHtml = `<h5 class="fw-bold text-primary mb-3 mt-4"><i class="ph ph-buildings me-2"></i> ${name} (Hedef Yapılar)</h5>`;
            let gNewApts = 0, gNewShops = 0;
            const newBlocks = document.querySelectorAll('.block-item-new');
            newBlocks.forEach(b => {
                const res = buildBlockSummary(b, false);
                newHtml += res.html;
                gNewApts += res.totalApts;
                gNewShops += res.totalShops;
            });
            newContainer.innerHTML = newHtml;
            newContainer.className = "mb-4";

            // Calculation (Fark)
            const diffApts = gNewApts - gOldApts;
            const diffShops = gNewShops - gOldShops;
            
            const aptIcon = diffApts >= 0 ? '<i class="ph ph-trend-up text-success"></i>' : '<i class="ph ph-trend-down text-danger"></i>';
            const shopIcon = diffShops >= 0 ? '<i class="ph ph-trend-up text-success"></i>' : '<i class="ph ph-trend-down text-danger"></i>';
            
            const aptColor = diffApts >= 0 ? 'text-success' : 'text-danger';
            const shopColor = diffShops >= 0 ? 'text-success' : 'text-danger';
            
            document.getElementById('firmStockAlert').innerHTML = `
                <div class="d-flex w-100 justify-content-between align-items-center p-2">
                    <div class="d-flex align-items-center">
                        <div class="bg-white rounded-circle p-2 shadow-sm me-3">
                            <i class="ph ph-scales fs-2 text-dark"></i>
                        </div>
                        <div>
                            <h5 class="fw-bold mb-1 text-dark">Kıyaslama ve Kazanç (Fark) Analizi</h5>
                            <div class="small text-muted">Eski (Mevcut) yapılar ile Yeni (Hedef) yapıların net kapasite farkı.</div>
                        </div>
                    </div>
                    <div class="text-end">
                        <div class="fs-5 fw-bold ${aptColor}">${aptIcon} ${diffApts > 0 ? '+'+diffApts : diffApts} Daire</div>
                        <div class="fs-5 fw-bold ${shopColor}">${shopIcon} ${diffShops > 0 ? '+'+diffShops : diffShops} Dükkan</div>
                    </div>
                </div>
            `;
            document.getElementById('firmStockAlert').className = "alert alert-secondary border-0 rounded-4 shadow-sm mb-4";
        }"""

content = re.sub(r'function prepareSummary\(\) \{.*?document\.getElementById\(\'firmStockShops\'\)\.innerText = Math\.max\(0, newShops - oldShops\);\s*\}', new_js, content, flags=re.DOTALL)

with open("GMK360.Web/Views/ConstructionProject/Create.cshtml", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated prepareSummary logic for Step 3 dashboard!")
