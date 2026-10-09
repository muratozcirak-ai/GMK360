import re

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Find Profile Menu Section
old_top_right = '''<!-- SAĞ: PROFİL MENÜSÜ -->
            <div class="d-flex gap-3 align-items-center">'''

new_top_right = '''<!-- SAĞ: PROFİL & BİLGİ MENÜSÜ -->
            <div class="d-flex gap-4 align-items-center">
                
                <!-- Hava Durumu (Bölge Seçimli) -->
                <div class="d-none d-md-flex align-items-center bg-light rounded-pill px-3 py-1 border shadow-sm">
                    <i class="bi bi-cloud-sun text-primary fs-5 me-2"></i>
                    <select class="form-select form-select-sm border-0 bg-transparent text-navy fw-bold py-0 ps-0 pe-4" style="box-shadow: none; width: auto; font-size: 0.85rem;" title="Şantiye Lokasyonu Seçin">
                        <option value="ist" selected>İstanbul, Ataşehir (22°C)</option>
                        <option value="ank">Ankara, Çankaya (18°C)</option>
                        <option value="izm">İzmir, Bornova (25°C)</option>
                    </select>
                </div>

                <!-- Giriş Saati ve Kullanıcı -->
                <div class="d-none d-lg-block text-end lh-sm border-end pe-3">
                    <div class="text-navy fw-bold" style="font-size: 0.85rem;"><i class="bi bi-person-check-fill text-success me-1"></i> @(User.Identity.IsAuthenticated ? User.Identity.Name : "Mimar Sinan")</div>
                    <div class="text-muted" style="font-size: 0.75rem;"><i class="bi bi-clock-history me-1"></i> Giriş: @DateTime.Now.ToString("HH:mm") | <span id="liveClock" class="fw-bold">@DateTime.Now.ToString("HH:mm:ss")</span></div>
                </div>

'''
content = content.replace(old_top_right, new_top_right)

# Add clock JS
js_inject = '''<script>
        function updateClock() {
            var now = new Date();
            var timeString = now.toLocaleTimeString('tr-TR', { hour12: false });
            var clockEl = document.getElementById('liveClock');
            if(clockEl) { clockEl.innerText = timeString; }
        }
        setInterval(updateClock, 1000);
    </script>
</body>'''
content = content.replace('</body>', js_inject)

with open(r'GMK360.Web\Views\Shared\_ConstructionLayout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
