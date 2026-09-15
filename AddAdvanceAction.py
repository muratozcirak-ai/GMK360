import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\DailyTimesheetsController.cs'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

new_action = '''
        [HttpPost]
        public async Task<IActionResult> RequestAdvance(int AgencyWorkerId, decimal Amount, int sourceProjectId)
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var worker = await _context.AgencyWorkers.FindAsync(AgencyWorkerId);
            if (worker == null) return NotFound();

            var advance = new GMK360.Core.Entities.Finance.AgencyCashTransaction
            {
                AgencyId = agencyId.Value,
                AgencyWorkerId = AgencyWorkerId,
                Amount = Amount,
                TransactionType = GMK360.Core.Entities.Finance.AgencyCashTransactionType.WorkerAdvance,
                Method = GMK360.Core.Entities.Finance.PaymentMethod.Cash,
                Status = GMK360.Core.Entities.Finance.PaymentStatus.Pending, // KASA ONAYI BEKLİYOR
                Description = $"{worker.FullName} için avans/harçlık talebi (Şantiye Onaylı)",
                PaymentDate = System.DateTime.UtcNow,
                HandledByUserId = _userManager.GetUserId(User)
            };

            _context.AgencyCashTransactions.Add(advance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{worker.FullName} için {Amount.ToString("C2")} avans talebi oluşturuldu. Merkez onayından sonra kasadan düşülecektir.";
            return RedirectToAction("ProjectTimesheet", new { id = sourceProjectId });
        }
'''

content = content.replace('    }\r\n}\r\n', new_action + '    }\r\n}\r\n').replace('    }\n}\n', new_action + '    }\n}\n')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
