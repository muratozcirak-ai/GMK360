import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Remove the description paragraph
content = re.sub(r'<p class="text-muted lead mx-auto mt-3" style="max-width: 800px;">.*?</p>', '', content, flags=re.DOTALL)

# Update Card 4: Emlak
content = content.replace('Emlak Ofisleri</h5>', 'Acenteler & Danışmanlar</h5>')

# Update Card 8: AI & Valuation
content = content.replace('Sözleşme analizi, beyanname hatırlatıcı', 'Sözleşme analizi, yapay zeka ile mülk değerlendirme, beyanname hatırlatıcı')

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
