import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix select options
content = content.replace('<option value="1">Devam Eden Proje (Aktif Şantiye)</option>', '<option value="2">Devam Eden Proje (Aktif Şantiye)</option>')
content = content.replace('<option value="0">Görüşmesi Devam Eden (Sözleşmesiz / Aday Proje)</option>', '<option value="1">Görüşmesi Devam Eden (Sözleşmesiz / Aday Proje)</option>')
content = content.replace('<option value="2">Tamamlanmış Proje (Referans / Portföy)</option>', '<option value="3">Tamamlanmış Proje (Referans / Portföy, Bina Yönetimi)</option>')

# Fix Javascript values
content = content.replace('if (status == "1")', 'if (status == "2")')
content = content.replace('} else if (status == "0")', '} else if (status == "1")')
content = content.replace('} else if (status == "2")', '} else if (status == "3")')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
