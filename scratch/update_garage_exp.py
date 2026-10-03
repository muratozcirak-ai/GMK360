import codecs

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = '''public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate)'''
replacement = '''public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer)'''
content = content.replace(target, replacement)

target2 = '''ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate)
            };'''
replacement2 = '''ExpenseDate = string.IsNullOrEmpty(expenseDate) ? DateTime.UtcNow : DateTime.Parse(expenseDate),
                OdometerAtExpense = odometer
            };
            
            // Eğer girilen KM, aracın mevcut KM'sinden büyükse aracın güncel KM'sini de otomatik güncelle
            if (odometer.HasValue)
            {
                var vehicle = await _context.CompanyVehicles.FindAsync(vehicleId);
                if (vehicle != null && odometer.Value > vehicle.CurrentKm)
                {
                    vehicle.CurrentKm = odometer.Value;
                }
            }'''
content = content.replace(target2, replacement2)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated AddExpense in controller')