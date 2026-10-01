import codecs
import re
path = 'GMK360.Web/Views/AdminCRM/UserDetails.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Remove the whole else if block for Guarantor/Supplier
content = re.sub(r'else if \(p.Role == GMK360\.Core\.Entities\.Construction\.StakeholderRole\.Supplier\)\s*\{\s*<span class="badge bg-warning text-dark">.*?</span>\s*\}', '', content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed enum')