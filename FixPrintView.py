import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\SubcontractorContract\PrintPreview.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('@model GMK360.Core.Entities.Construction.SubcontractorContract', '@model GMK360.Core.Entities.Finance.SubcontractorContract')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
