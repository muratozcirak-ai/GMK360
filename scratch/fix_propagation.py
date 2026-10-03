import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add event.stopPropagation() to all buttons in the main row that shouldn't toggle the row.
# 1. Yönet button
content = content.replace('onclick="openManageModal(@doc.Id)"', 'onclick="event.stopPropagation(); openManageModal(@doc.Id)"')
# 2. İhaleye Git a tag
content = content.replace('class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Satınalma & İhale Yönetimi"', 'class="btn btn-sm btn-info rounded-pill px-2 shadow-sm text-white fw-bold" title="Satınalma & İhale Yönetimi" onclick="event.stopPropagation();"')
# 3. Teklif submit button
content = content.replace('onclick="event.stopPropagation();"', '') # Reset just in case
content = content.replace('<button type="submit" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold"', '<button type="submit" onclick="event.stopPropagation();" class="btn btn-sm btn-warning rounded-pill px-2 shadow-sm text-dark fw-bold"')
# 4. İhaleye Git again
content = content.replace('title="Satınalma & İhale Yönetimi"', 'title="Satınalma & İhale Yönetimi" onclick="event.stopPropagation();"')


with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)