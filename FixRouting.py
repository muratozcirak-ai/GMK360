import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Routing\SeoRouteTransformer.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add the new controllers to the string
old_list = '"SupplierCurrentAccounts", "TaxAssistant"'
new_list = '"SupplierCurrentAccounts", "SupplierCurrentAccount", "SubcontractorContract", "Timesheet", "Finance", "PartnerPortal", "AgencyStaffFinance", "AgencyStaff", "AgencyPhonebook", "Agenda", "TaxAssistant"'

content = content.replace(old_list, new_list)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
