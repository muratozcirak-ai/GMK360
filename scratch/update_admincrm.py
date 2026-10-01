import io
import re

filepath = r'GMK360.Web\Controllers\AdminCRMController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

injection = """
            // B2B Shadow Accounts (Potansiyel Firmalar)
            var b2bShadows = await _context.Set<GMK360.Core.Entities.B2B.B2BNetworkContact>()
                .Include(c => c.OwnerAgency)
                .OrderByDescending(c => c.Id)
                .Take(50)
                .ToListAsync();
            ViewBag.B2BShadows = b2bShadows;

            var finalUsers = await usersQuery.OrderByDescending(u => u.Id).Take(100).ToListAsync();
"""

content = content.replace("var finalUsers = await usersQuery.OrderByDescending(u => u.Id).Take(100).ToListAsync();", injection)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Updated AdminCRMController")
