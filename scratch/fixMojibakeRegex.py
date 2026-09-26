import re

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'r', encoding='utf-8') as f:
    html = f.read()

# I will just replace all versions of the corrupted strings I see.
# In the file it is Ak Konular
html = re.sub(r'A\w*k Konular / \w*n Haz\w*rl\w*k Talepleri', 'Açık Konular / Ön Hazırlık Talepleri', html)
html = re.sub(r'Yeni Konu A\w+', 'Yeni Konu Aç', html)
html = re.sub(r'Hen\w*z tart\w*maya a\w*lm\w* bir konu veya \w*n talep bulunmuyor\.', 'Henüz tartışmaya açılmış bir konu veya ön talep bulunmuyor.', html)

with open('GMK360.Web/Views/ConstructionProject/Details.cshtml', 'w', encoding='utf-8') as f:
    f.write(html)
