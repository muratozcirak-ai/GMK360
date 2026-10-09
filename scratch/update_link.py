import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# I will just replace the specific block for the first card.
old_block = '''<a href="#" class="text-decoration-none fw-bold text-primary mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 2. Bina, Site & AVM Yönetimi -->'''
        
new_block = '''<a href="/Modules/Insaat" target="_blank" class="text-decoration-none fw-bold text-primary mt-auto d-inline-block" style="font-size: 0.8rem;">Sistemi Keşfet <i class="bi bi-arrow-right ms-1"></i></a>
                </div>
            </div>
        </div>

        <!-- 2. Bina, Site & AVM Yönetimi -->'''

content = content.replace(old_block, new_block)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
