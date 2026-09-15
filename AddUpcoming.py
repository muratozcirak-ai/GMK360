import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\FinanceController.cs'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

upcoming_method = '''
        public async Task<IActionResult> UpcomingPayments()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            // Sadece vadesi gelmiş ve henüz ödenmemiş tedarikçi ödemeleri (çek/senet vb.)
            var payments = await _context.SupplierPayments
                .Include(p => p.SupplierCurrentAccount).ThenInclude(s => s.PhonebookContact)
                .Where(p => p.SupplierCurrentAccount.AgencyId == agencyId.Value && p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .ToListAsync();

            return View(payments);
        }
'''

content = content.replace('    }\n}\n', upcoming_method + '    }\n}\n')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
