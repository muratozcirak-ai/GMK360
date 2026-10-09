import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the Hero text
old_hero = '''<h1 class="display-5 fw-bolder mb-5 text-white" style="letter-spacing: -1px; text-shadow: 0 4px 15px rgba(0,0,0,0.8);">
            Sektörün Değişen Yüzü: <br/> 
            <span class="text-orange">Dijital Ekosistem</span>
        </h1>'''

new_hero = '''<h1 class="display-4 fw-bolder mb-3 text-white" style="letter-spacing: -1px; text-shadow: 0 4px 15px rgba(0,0,0,0.8);">
            Topraktan Yaşama: <br/> 
            <span class="text-orange">Gayrimenkulün Dijital İkizi</span>
        </h1>
        <p class="lead text-white-50 mb-5 mx-auto" style="max-width: 700px;">Şantiyenizden site yönetimine, malzeme tedariğinden kira tahsilatına kadar tüm gayrimenkul döngüsünü 360 derece kapalı ekosistemde yönetin.</p>'''

content = content.replace(old_hero, new_hero)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
