import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

new_hero = '''<!-- MASTER HERO WITH NEW BANNER IMAGE -->
<div class="text-white text-center" style="background-color: #091324; position: relative;">
    <img src="/images/hero-bg.jpg" alt="Ecosystem" style="width: 100%; height: auto; max-height: 75vh; object-fit: contain;">
    <!-- Koyu renkli hafif bir maske -->
    <div style="position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(9, 19, 36, 0.4);"></div>
    
    <div class="position-absolute top-50 start-50 translate-middle w-100 z-1">
        <h3 class="fw-bold text-white-50 opacity-50" style="letter-spacing: 2px; text-shadow: 0 2px 10px rgba(0,0,0,0.8);">YENİ İÇERİK TASARIMI BEKLENİYOR</h3>
    </div>
</div>
'''

content = re.sub(r'<!-- MASTER HERO.*?</div>\s*</div>\s*</div>', new_hero + '\n</div>', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
