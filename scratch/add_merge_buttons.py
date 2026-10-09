import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# 1. Add Top Button
target_top = '<a href="/ConstructionProject/Create/@Model.Id" target="_blank" class="btn btn-warning text-dark fw-bold ms-2"><i class="bi bi-magic"></i> Sihirbaza D'

# Let's search by a safer string
idx_top = content.find('Sihirbaza D')
if idx_top != -1:
    idx_top_start = content.rfind('<a href', 0, idx_top)
    
    # We will insert BEFORE the Sihirbaza Don button so it reads: [Tevhit] [Sihirbaza Don]
    top_btn_html = '''<a href="#" class="btn btn-dark fw-bold ms-2 shadow-sm" data-bs-toggle="modal" data-bs-target="#mergeBlocksModal" title="Blokları/Parselleri Birleştir">
            <i class="bi bi-intersect"></i> Tevhit (Birleştir)
        </a>
        '''
    content = content[:idx_top_start] + top_btn_html + content[idx_top_start:]


# 2. Add Warning to Blocks accordion
target_blocks = 'id="accordionBloklar"'
idx_blocks = content.find(target_blocks)
if idx_blocks != -1:
    # Find the accordion-body
    idx_body = content.find('class="accordion-body', idx_blocks)
    if idx_body != -1:
        idx_body_close = content.find('>', idx_body) + 1
        
        warning_html = '''
                <!-- TEVHİT UYARISI -->
                <div class="alert alert-warning border-0 shadow-sm d-flex align-items-center mb-4 bg-warning bg-opacity-10">
                    <i class="bi bi-exclamation-triangle-fill fs-3 text-warning me-3"></i>
                    <div>
                        <h6 class="fw-bold mb-1 text-dark">Tevhit (Birleştirme) Gerekli Olabilir!</h6>
                        <p class="mb-0 small text-dark">Bu projede birden fazla blok tespit edildi. Eğer bu bloklar <b>Bitişik Nizam</b> ise veya hafriyat/temel işlemleri tek seferde yapılacaksa, Faz (Aşama) yönetiminde patlamamak için bu binaları tek bir hedefte <b>birleştirmeniz (Tevhit)</b> zorunludur.</p>
                    </div>
                    <button class="btn btn-warning text-dark fw-bold ms-auto text-nowrap shadow-sm" data-bs-toggle="modal" data-bs-target="#mergeBlocksModal"><i class="bi bi-intersect"></i> Şimdi Birleştir</button>
                </div>
'''
        content = content[:idx_body_close] + warning_html + content[idx_body_close:]

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)

print("Buttons added!")
