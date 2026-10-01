import codecs
path = 'GMK360.Web/Views/AdminCRM/UserDetails.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix 1: CreatedAt -> remove it or use something else. Let's just remove the registration date block if it doesn't exist.
import re
content = re.sub(r'<div class="mb-3">\s*<small class="text-muted d-block text-uppercase fw-bold" style="font-size: 0\.75rem;">Sisteme Kayıt Tarihi</small>\s*<div class="fs-6"><i class="bi bi-calendar-date text-secondary me-2"></i>@Model\.CreatedAt[^<]+</div>\s*</div>', '', content)

# Fix 2: StakeholderRole.Guarantor -> StakeholderRole.Supplier
content = content.replace('StakeholderRole.Guarantor', 'StakeholderRole.Supplier')

# Fix 3: r.Agency.Title -> r.Agency.Name (Assuming Agency has Name)
content = content.replace('@r.Agency.Title', '@r.Agency.Name')

# Fix 4: r.Title -> r.Role or r.JobTitle or we can just remove it
content = content.replace('<td>@r.Title</td>', '<td>@r.AgencyRole</td>')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed Razor errors')