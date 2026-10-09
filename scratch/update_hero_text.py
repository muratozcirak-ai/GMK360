import re
with open(r'GMK360.Web\Views\Modules\Insaat.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

content = content.replace('Hata payını sıfırlayın, karlılığınızı koruyun.', 'Yanlış uygulama riskini minimuma düşürün ve prestijinizi koruyun.')

with open(r'GMK360.Web\Views\Modules\Insaat.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
