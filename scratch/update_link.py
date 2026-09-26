import sys
filepath = 'GMK360.Web/Views/AdminLegalDocument/Index.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """        <div>
            <a href="/ModuleDocumentRule/Index" class="btn btn-warning rounded-pill px-4 fw-bold shadow-sm me-2">
                <i class="ph ph-tree-structure me-1"></i> Modül Kurallarını Yönet
            </a>
            <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" data-bs-toggle="modal" data-bs-target="#addModal">
                <i class="ph ph-plus me-1"></i> Yeni Evrak Ekle
            </button>
        </div>"""

content = content.replace("""        <button class="btn btn-primary rounded-pill px-4 fw-bold shadow-sm" data-bs-toggle="modal" data-bs-target="#addModal">
            <i class="ph ph-plus me-1"></i> Yeni Evrak Ekle
        </button>""", replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
