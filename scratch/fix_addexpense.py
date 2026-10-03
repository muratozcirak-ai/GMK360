import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> AddExpense\(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int\? odometer\)'
replacement = r'public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer, Microsoft.AspNetCore.Http.IFormFile? receiptPhoto)'
content = re.sub(target, replacement, content)

target_body = r'OdometerAtExpense = odometer\s*\};\s*//'
replacement_body = '''OdometerAtExpense = odometer
            };
            
            if (receiptPhoto != null && receiptPhoto.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(receiptPhoto.FileName);
                var uploadsFolder = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "garage");
                System.IO.Directory.CreateDirectory(uploadsFolder);
                var filePath = System.IO.Path.Combine(uploadsFolder, fileName);
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await receiptPhoto.CopyToAsync(stream);
                }
                exp.PhotoPath = "/uploads/garage/" + fileName;
            }
            
            //'''
content = re.sub(target_body, replacement_body, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('AddExpense method updated to handle photo uploads.')