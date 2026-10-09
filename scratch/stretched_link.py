import re
with open(r'GMK360.Web\Views\Home\Index.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Add stretched-link class to all "Sistemi Keşfet" links in the 8 modules grid
content = content.replace('class="text-decoration-none fw-bold text-primary mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-primary mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-info mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-info mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-success mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-success mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-orange mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-orange mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-secondary mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-warning mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-warning mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-danger mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-danger mt-auto d-inline-block stretched-link"')
content = content.replace('class="text-decoration-none fw-bold text-dark mt-auto d-inline-block"', 'class="text-decoration-none fw-bold text-dark mt-auto d-inline-block stretched-link"')

with open(r'GMK360.Web\Views\Home\Index.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
