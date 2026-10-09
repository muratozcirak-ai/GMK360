with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    lines = f.readlines()

new_lines = []
skip = False
for line in lines:
    if 'data-bs-target="#mergeBlocksModal"' in line or '<i class="bi bi-intersect"></i> Tevhit' in line:
        continue # skip the a tag lines
    
    if '<!-- TEVH' in line and 'MODALI -->' in line:
        skip = True
    
    if skip:
        if '</div>' in line and '<!-- End Modal -->' in line: # Try to find end, or just use a fixed count
            pass
    if not skip:
        new_lines.append(line)
        
with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.writelines(new_lines)
