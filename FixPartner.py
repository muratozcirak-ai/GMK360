import codecs

# Fix Controller
filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\PartnerPortalController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('contact.Type == GMK360.Core.Entities.Enums.PhonebookContactType.Supplier', 'contact.ContactType == 3')
content = content.replace('contact.FullName', 'contact.Name')
content = content.replace('c.Status == GMK360.Core.Entities.Enums.SubcontractorContractStatus.Active', 'c.IsActive')
content = content.replace('c.ProgressPayments.OrderByDescending(p => p.PeriodEndDate)', 'c.Hakedisler.OrderByDescending(p => p.HakedisDate)')
content = content.replace('Include(c => c.ProgressPayments', 'Include(c => c.Hakedisler')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)

# Fix View
filepath_view = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\PartnerPortal\Contracts.cshtml'
with codecs.open(filepath_view, 'r', 'utf-8-sig') as f:
    view = f.read()

view = view.replace('contract.WorkDescription', 'contract.Title')
view = view.replace('@contract.Status', '@(contract.IsActive ? "Aktif" : "Pasif")')
view = view.replace('contract.ProgressPayments', 'contract.Hakedisler')
view = view.replace('hakedis.PeriodEndDate', 'hakedis.HakedisDate')
view = view.replace('hakedis.NetPayableAmount', '(hakedis.ClaimAmount - hakedis.DeductionAmount)')

with codecs.open(filepath_view, 'w', 'utf-8-sig') as f:
    f.write(view)
