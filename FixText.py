import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8') as f:
    content = f.read()

content = content.replace('Şantiyeyi Başlat', 'Projeyi Oluştur')
content = content.replace('Proje başlatıldıktan sonra', 'Proje oluşturulduktan sonra')

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
