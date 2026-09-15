import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('<i class="bi bi-boxes me-2 text-primary"></i> Şantiye Depoları (Demirbaş)', '<i class="bi bi-boxes me-2 text-primary"></i> Merkez Depo & Envanter')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
