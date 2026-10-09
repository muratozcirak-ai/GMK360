import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace the first module's link
content = re.sub(r'href="#"(.*?)Sistemi Keşfet(.*?)İnşaat, Hafriyat & Yıkım', r'href="/Modules/Insaat" target="_blank"\1Sistemi Keşfet\2İnşaat, Hafriyat & Yıkım', content, flags=re.DOTALL)
# Wait, the <a> tag is inside the card AFTER the h6.
