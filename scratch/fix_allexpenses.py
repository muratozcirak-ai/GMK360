import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/AllExpenses.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@item.Description', '@(item.ReceiptNumber ?? "Fiş No Yok")')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed AllExpenses.cshtml.')