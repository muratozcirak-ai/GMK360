import re

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_nav = '''<nav class="d-none d-md-flex gap-4 fw-medium text-secondary">
                <a asp-controller="Home" asp-action="Index" class="text-decoration-none text-dark hover-orange fw-bold">Ana Sayfa</a>
                <a href="#" class="text-decoration-none text-dark hover-orange fw-bold">Biz Kimiz</a>
                <a href="#" class="text-decoration-none text-dark hover-orange fw-bold">İletişim</a>
            </nav>'''

content = re.sub(r'<nav class="d-none d-md-flex gap-4 fw-medium text-secondary">.*?</nav>', new_nav, content, flags=re.DOTALL)

new_right_side = '''<div class="d-flex gap-3 align-items-center">
                <!-- Tarih ve Saat -->
                <div class="d-none d-lg-flex align-items-center text-muted fw-bold border-end pe-3 me-1" style="font-size: 0.9rem;">
                    <i class="bi bi-calendar3 me-2 text-orange"></i>
                    <span id="navLiveDate"></span>
                    <i class="bi bi-clock ms-3 me-2 text-primary"></i>
                    <span id="navLiveTime"></span>
                </div>
                
                @if (User.Identity.IsAuthenticated)'''

content = re.sub(r'<div class="d-flex gap-3 align-items-center">\s*@if \(User.Identity.IsAuthenticated\)', new_right_side, content, flags=re.DOTALL)

# Add the JS for the clock at the end of the body
clock_js = '''<script>
        function updateNavClock() {
            const now = new Date();
            const dateStr = now.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' });
            const timeStr = now.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
            
            const dateEl = document.getElementById('navLiveDate');
            const timeEl = document.getElementById('navLiveTime');
            if(dateEl) dateEl.innerText = dateStr;
            if(timeEl) timeEl.innerText = timeStr;
        }
        setInterval(updateNavClock, 1000);
        updateNavClock();
    </script>
</body>'''

content = content.replace('</body>', clock_js)

with open(r'GMK360.Web\Views\Shared\_Layout.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
