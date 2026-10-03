import codecs
import re

path = 'GMK360.Web/Views/Shared/_ConstructionLayout.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('Proje Genel Giderleri', 'Şirket Genel Giderleri')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)