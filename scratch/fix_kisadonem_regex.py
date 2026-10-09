import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the title and the link
content = re.sub(
    r'<h6 class="fw-bold text-navy mb-0 lh-sm">Kısa Dönem Kiralık</h6>\s*</div>\s*<p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">.*?</p>\s*<a href="[^"]*".*?class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block stretched-link".*?>Sistemi Keşfet',
    r'<h6 class="fw-bold text-navy mb-0 lh-sm">Günlük Kiralık & Pansiyon</h6>\n                    </div>\n                    <p class="text-muted mb-3" style="font-size: 0.8rem; line-height: 1.4;">Check-in takvimi, KBS (Emniyet) bildirimi, temizlik yönlendirme ve dinamik fiyatlama ile geliri katlayın.</p>\n                    <a href="/Modules/KisaDonem" target="_blank" class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet',
    content,
    flags=re.DOTALL
)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
