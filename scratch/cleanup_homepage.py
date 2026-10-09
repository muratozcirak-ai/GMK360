import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Replace Usta Link just in case it failed
content = content.replace('<a href="#" class="text-decoration-none fw-bold text-danger mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet', '<a href="/Modules/Usta" target="_blank" class="text-decoration-none fw-bold text-danger mt-auto d-inline-block stretched-link" style="font-size: 0.8rem;">Sistemi Keşfet')

# Find the end of the modules row and remove everything after it up to the <style> tag
pattern = r'(</div>\s*<!-- End of 8-card row -->\s*</div>\s*<!-- End of container-fluid -->\s*).*?(<style>)'
content = re.sub(pattern, r'\1\2', content, flags=re.DOTALL)

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
