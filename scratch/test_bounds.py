import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will find the ESKİ BİNA and YENİ BİNA sections and rewrite them side-by-side.
# To not break the razor logic, I'll just look for the accordion items.
# Let's find the start of the ESKİ BİNA accordion.
start_str = '<!-- ESK'
start_idx = content.find(start_str)
if start_idx == -1:
    start_idx = content.find('<div class="accordion-item border-0 rounded-4 overflow-hidden mb-3 shadow-sm">')

# Let's find the end of the YENİ BİNA accordion.
end_str = '<!-- PROJEDEKI BLOKLAR / BİNALAR -->'
end_idx = content.find('Projedeki Bloklar / Binalar')
if end_idx != -1:
    end_idx = content.rfind('<div class="d-flex', 0, end_idx)

print('Start:', start_idx, 'End:', end_idx)
