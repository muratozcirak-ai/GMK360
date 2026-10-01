import codecs

path = 'GMK360.Web/Controllers/TimesheetController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target_method = '''        [HttpGet]
        public async Task<IActionResult> PendingAdvances()'''

new_method = '''        [HttpGet]
        public async Task<IActionResult> PendingAdvances()
        {
            var agencyId = await GetUserAgencyIdAsync();
            if (agencyId == null) return Unauthorized();

            var vm = new GMK360.Web.Models.Finance.UnifiedFinanceDashboardViewModel();

            // 1. Pending Advances
            vm.PendingAdvances = await _context.AgencyStaffAdvances
                .Include(a => a.Worker)
                .Where(a => a.Worker != null && a.Worker.AgencyId == agencyId.Value && a.Status == AdvanceStatus.Pending)
                .OrderBy(a => a.RequestDate)
                .ToListAsync();

            // 2. White Collar
            var whiteCollars = await _context.AgencyConsultants
                .Include(c => c.User)
                .Where(c => c.AgencyId == agencyId.Value && c.IsActive)
                .ToListAsync();

            foreach(var w in whiteCollars)
            {
                vm.WhiteCollars.Add(new GMK360.Web.Models.Finance.WhiteCollarSalaryItem {
                    FullName = w.User != null ? (w.User.FirstName + " " + w.User.LastName) : "İsimsiz",
                    RoleName = w.Role.ToString(),
                    NetSalary = w.MonthlySalary,
                    SgkCost = w.MonthlySgkCost
                });
                vm.TotalWhiteCollarNet += w.MonthlySalary;
                vm.TotalWhiteCollarSgk += w.MonthlySgkCost;
            }

            // 3. Blue Collar (Current Month Timesheets)
            var currentMonth = DateTime.Today.Month;
            var currentYear = DateTime.Today.Year;
            
            var workers = await _context.AgencyWorkers
                .Include(w => w.Timesheets.Where(t => t.WorkDate.Month == currentMonth && t.WorkDate.Year == currentYear))
                .Where(w => w.AgencyId == agencyId.Value && w.IsActive)
                .ToListAsync();

            foreach(var bw in workers)
            {
                var timesheets = bw.Timesheets.ToList();
                var daysWorked = timesheets.Count(t => t.AttendanceStatus == "Tam Gün");
                var halfDays = timesheets.Count(t => t.AttendanceStatus == "Yarım Gün");
                
                var earnedWage = (daysWorked * bw.DefaultDailyWage) + (halfDays * bw.DefaultDailyWage / 2);
                var fieldExpenses = timesheets.Sum(t => t.PendingFieldExpense);

                if (daysWorked > 0 || halfDays > 0 || fieldExpenses > 0)
                {
                    vm.BlueCollars.Add(new GMK360.Web.Models.Finance.BlueCollarWageItem {
                        FullName = bw.FullName,
                        Profession = bw.Profession,
                        DaysWorked = daysWorked,
                        EarnedWages = earnedWage,
                        TotalPendingFieldExpenses = fieldExpenses
                    });
                    vm.TotalBlueCollarWage += earnedWage;
                    vm.TotalFieldExpenses += fieldExpenses;
                }
            }

            return View(vm);
        }'''

# Replace the old PendingAdvances method (using string slicing to drop the old body)
idx_start = content.find(target_method)
if idx_start != -1:
    idx_end = content.find('[HttpPost]', idx_start)
    if idx_end != -1:
        content = content[:idx_start] + new_method + '\n\n' + content[idx_end:]

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated TimesheetController for Unified Dashboard!')