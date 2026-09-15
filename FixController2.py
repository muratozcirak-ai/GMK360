import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\PartnerPortalController.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('SubcontractorId == contact.Id', 'PhonebookContactId == contact.Id')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)

filepath_view = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal\Contracts.cshtml'
with codecs.open(filepath_view, 'r', 'utf-8-sig') as f:
    content_view = f.read()

content_view = content_view.replace('@contract.Subject', '@contract.Title')

with codecs.open(filepath_view, 'w', 'utf-8-sig') as f:
    f.write(content_view)
