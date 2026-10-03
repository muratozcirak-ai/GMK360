import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# 1. Remove disabled from Teklif button
content = content.replace('@(isDependent && !isReadyToApply ? "disabled" : "")', '')

# 2. Fix Yönet button
target_yonet = r'<button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="event\.stopPropagation\(\); openManageModal\(@doc\.Id\)" @\(isDependent && !isReadyToApply \? "disabled title=\'Önce alt ön koşul evrakları tamamlanmalıdır\'" : ""\)>'
replacement_yonet = '''<button class="btn btn-sm btn-outline-primary rounded-pill px-3 fw-bold shadow-sm" onclick="event.stopPropagation(); openManageModal(@doc.Id)">'''
content = re.sub(target_yonet, replacement_yonet, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)