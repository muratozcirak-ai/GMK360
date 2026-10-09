import re
with open(r'GMK360.Web\Views\Modules\Insaat.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Hero Section tightening
content = content.replace('min-height: 400px;', 'min-height: 250px;')
content = content.replace('py-5 mt-4', 'py-4 mt-2')
content = content.replace('mb-5 pe-lg-5', 'mb-4 pe-lg-5')

# 2. Content gaps tightening
content = content.replace('py-5 my-5', 'py-4 my-4')
content = content.replace('mb-5 pb-5', 'mb-4 pb-4')
content = content.replace('mb-4 mb-lg-0', 'mb-3 mb-lg-0')
content = content.replace('mb-4 border-bottom', 'mb-3 border-bottom')

with open(r'GMK360.Web\Views\Modules\Insaat.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
