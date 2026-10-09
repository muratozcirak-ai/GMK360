import codecs
import re

path = 'GMK360.Web/Controllers/ProjectFinanceController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('ToString("N2") + " ?"', 'ToString("N2") + " ₺"')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)