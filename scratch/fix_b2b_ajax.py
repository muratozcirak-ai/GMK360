import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the select tag to add an ID
pattern1 = r'<select name="projectId" class="form-select border-primary" required>'
replacement1 = r'<select name="projectId" id="quoteProjectId" class="form-select border-primary" required>'
content = re.sub(pattern1, replacement1, content)

# Inject the stats card after the closing div of the select container
# The container is <div class="mb-3">...<select...>...</select></div>
pattern2 = r'(<select name="projectId" id="quoteProjectId" class="form-select border-primary" required>.*?</select>\s*</div>)'
stats_card = r"""\1
                      <div id="projectStatsCard" class="alert alert-info mt-2 mb-3 d-none">
                          <div class="d-flex align-items-center mb-2">
                              <i class="bi bi-building fs-5 me-2"></i>
                              <h6 class="mb-0 fw-bold">Seçili Proje / Yapı Bilgileri</h6>
                          </div>
                          <div class="row g-2 small">
                              <div class="col-6"><span class="text-muted">Bina Yaşı:</span> <strong id="statBinaYasi">-</strong></div>
                              <div class="col-6"><span class="text-muted">Kat Sayısı:</span> <strong id="statKatSayisi">-</strong></div>
                              <div class="col-6"><span class="text-muted">Bodrum:</span> <strong id="statBodrum">-</strong></div>
                              <div class="col-6"><span class="text-muted">Daire / Dükkan:</span> <strong id="statDaireDukkan">-</strong></div>
                              <div class="col-6"><span class="text-muted">Arsa (m²):</span> <strong id="statArsa">-</strong></div>
                              <div class="col-6"><span class="text-muted">Taban (m²):</span> <strong id="statTaban">-</strong></div>
                          </div>
                      </div>"""
content = re.sub(pattern2, stats_card, content, flags=re.DOTALL)

# Inject the JS code inside the <script> block
script_code = r"""
<script>
    document.addEventListener('DOMContentLoaded', function() {
        var projectSelect = document.getElementById('quoteProjectId');
        if (projectSelect) {
            projectSelect.addEventListener('change', function() {
                var projectId = this.value;
                var statsCard = document.getElementById('projectStatsCard');
                
                if (!projectId) {
                    statsCard.classList.add('d-none');
                    return;
                }
                
                fetch('/ConstructionProject/GetOldBuildingStats?projectId=' + projectId)
                    .then(response => response.json())
                    .then(data => {
                        document.getElementById('statBinaYasi').textContent = data.binaYasi > 0 ? data.binaYasi + ' Yıl' : 'Belirtilmemiş';
                        document.getElementById('statKatSayisi').textContent = data.toplamKat > 0 ? data.toplamKat : '-';
                        document.getElementById('statBodrum').textContent = data.bodrumKatSayisi > 0 ? data.bodrumKatSayisi : 'Yok';
                        document.getElementById('statDaireDukkan').textContent = (data.daireSayisi || 0) + ' / ' + (data.dukkanSayisi || 0);
                        document.getElementById('statArsa').textContent = data.arsaAlani > 0 ? data.arsaAlani : '-';
                        document.getElementById('statTaban').textContent = data.tabanOturumu > 0 ? data.tabanOturumu : '-';
                        
                        statsCard.classList.remove('d-none');
                    })
                    .catch(err => {
                        console.error("Proje bilgileri çekilemedi:", err);
                        statsCard.classList.add('d-none');
                    });
            });
        }
    });

    function addMaterialRow() {"""

content = content.replace('<script>\n    function addMaterialRow() {', script_code)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("B2B Index.cshtml updated with AJAX.")
