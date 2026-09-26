import re

filepath = r'GMK360.Web\Views\B2BPurchasing\Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('Bina Yaşı:', 'Yapım Yılı:')
# Modify the JS logic: data.binaYasi > 0 ? data.binaYasi + ' Yıl' : 'Belirtilmemiş' 
# to just data.binaYasi
content = content.replace("data.binaYasi + ' Yıl'", "data.binaYasi")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("B2B labels updated.")
