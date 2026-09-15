import codecs

# Fix Contracts.cshtml
filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal\Contracts.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()
content = content.replace('GMK360.Core.Entities.Construction.SubcontractorContract', 'GMK360.Core.Entities.Finance.SubcontractorContract')
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)

# Fix Index.cshtml
filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal\Index.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()
content = content.replace('GMK360.Core.Entities.Construction.SubcontractorContract', 'GMK360.Core.Entities.Finance.SubcontractorContract')
with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
