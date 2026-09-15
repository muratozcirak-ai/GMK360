import codecs

filepath_c = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DocumentArchiveController.cs'
with codecs.open(filepath_c, 'r', 'utf-8-sig') as f:
    content_c = f.read()
content_c = content_c.replace('FileUrl =', 'DocumentUrl =')
with codecs.open(filepath_c, 'w', 'utf-8-sig') as f:
    f.write(content_c)

filepath_v = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\DocumentArchive\Index.cshtml'
with codecs.open(filepath_v, 'r', 'utf-8-sig') as f:
    content_v = f.read()
content_v = content_v.replace('@doc.FileUrl', '@doc.DocumentUrl')
with codecs.open(filepath_v, 'w', 'utf-8-sig') as f:
    f.write(content_v)
