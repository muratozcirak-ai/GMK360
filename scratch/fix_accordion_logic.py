import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Clean the stray brackets
stray_pattern = r'<!-- TASLAK / KONU TARTIŞMA BÖLÜMÜ -->\s*\}\s*</div>\s*</div>\s*</div>\s*\}\s*'
content = re.sub(stray_pattern, '', content)
# Try ascii just in case
stray_pattern2 = r'<!-- TASLAK / KONU TARTI\?MA BLM -->\s*\}\s*</div>\s*</div>\s*</div>\s*\}\s*'
content = re.sub(stray_pattern2, '', content)

# I will also just manually replace the exact string from the file
exact_stray = '''<!-- TASLAK / KONU TARTI?MA BLM -->






                    }
                </div>
            </div>
        </div>
    }'''
# To handle encoding issues in python matching, I will use a simple slicing if possible.
# Let's just find the first '<!-- Custom Doc Modal at the bottom -->' and remove everything between '</div>' and it.
modal_idx = content.find('<!-- Custom Doc Modal at the bottom -->')
if modal_idx != -1:
    # Find the closing tag of accordionOzet
    ozet_close = content.rfind('</div>', 0, modal_idx)
    # The stray text is right before modal_idx.
    stray_start = content.find('<!-- TASLAK')
    if stray_start != -1 and stray_start < modal_idx:
        content = content[:stray_start] + '\n\n' + content[modal_idx:]


# 2. Collapse Bloklar by default
bloklar_header = '''<button class="accordion-button bg-light text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseBloklar" aria-expanded="true" aria-controls="collapseBloklar">'''
new_bloklar_header = '''<button class="accordion-button collapsed bg-light text-dark fw-bold" type="button" data-bs-toggle="collapse" data-bs-target="#collapseBloklar" aria-expanded="false" aria-controls="collapseBloklar">'''
content = content.replace(bloklar_header, new_bloklar_header)

bloklar_collapse = '''<div id="collapseBloklar" class="accordion-collapse collapse show" aria-labelledby="headingBloklar">'''
new_bloklar_collapse = '''<div id="collapseBloklar" class="accordion-collapse collapse" aria-labelledby="headingBloklar">'''
content = content.replace(bloklar_collapse, new_bloklar_collapse)


# 3. Add JS snippet at the end of the file to link the two accordions
js_snippet = '''

@section Scripts {
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            var colOzet = document.getElementById('collapseOzet');
            var colBloklar = document.getElementById('collapseBloklar');
            
            if (colOzet && colBloklar) {
                colBloklar.addEventListener('show.bs.collapse', function () {
                    var bsCollapse = bootstrap.Collapse.getInstance(colOzet);
                    if(bsCollapse) bsCollapse.hide();
                    else new bootstrap.Collapse(colOzet, {toggle: false}).hide();
                });
                
                colOzet.addEventListener('show.bs.collapse', function () {
                    var bsCollapse = bootstrap.Collapse.getInstance(colBloklar);
                    if(bsCollapse) bsCollapse.hide();
                    else new bootstrap.Collapse(colBloklar, {toggle: false}).hide();
                });
            }
        });
    </script>
}
'''
if 'document.getElementById(\'collapseOzet\');' not in content:
    content += js_snippet

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
print("done")
