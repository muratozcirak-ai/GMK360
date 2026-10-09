import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

pattern = re.compile(
    r'(\s*</div>\s*<!-- End of 50/50 row -->\s*\})\s*</div>\s*</div>\s*</div>\s*</div>\s*else\s*\{\s*(<div class="row g-4 mb-4">.*?</div>\s*</div>\s*</div>\s*)\}'
    , re.DOTALL
)

def replacer(m):
    return m.group(1) + '''
                else
                {
                    ''' + m.group(2) + '''}
            </div>
        </div>
    </div>
</div>'''

new_content = pattern.sub(replacer, content)

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(new_content)
print("done")
