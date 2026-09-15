import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

content = content.replace('Şantiye Depoları', 'Merkez Depo (Demirbaş)')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
