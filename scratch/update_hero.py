import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_hero = '''<!-- MASTER HERO WITH NEW BANNER IMAGE -->
<div class="text-white text-center" style="background: url('/images/hero-bg.jpg') no-repeat center center; background-size: cover; position: relative; overflow: hidden; min-height: 400px; display: flex; align-items: center; justify-content: center; box-shadow: inset 0 0 100px rgba(0,0,0,0.8);">
    <!-- Koyu renkli hafif bir maske (overlay) metinlerin daha iyi okunması için eklenebilir -->
    <div style="position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(15, 23, 42, 0.4);"></div>

    <div class="container position-relative z-1 py-5">
        <!-- İÇERİK BOŞALTILDI -->
        <!-- Yeni vizyona göre içerik buraya gelecek -->
        <h3 class="fw-bold text-white-50 opacity-50" style="letter-spacing: 2px;">YENİ İÇERİK TASARIMI BEKLENİYOR</h3>
    </div>
</div>
'''

content = re.sub(r'<!-- MASTER HERO.*?</div>\s*</div>\s*</div>\s*</div>', new_hero, content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
