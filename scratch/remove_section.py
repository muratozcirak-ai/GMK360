with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    lines = f.readlines()

# We want to remove lines 220 to 274 basically.
# We can just look for '<!-- ORTAK VİZYON & SİSTEM FELSEFESİ -->' and remove until the next '<style>'

start_idx = -1
end_idx = -1

for i, line in enumerate(lines):
    if '<!-- ORTAK VİZYON' in line:
        start_idx = i
    if '<style>' in line and start_idx != -1:
        end_idx = i
        break

if start_idx != -1 and end_idx != -1:
    del lines[start_idx:end_idx]

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.writelines(lines)
