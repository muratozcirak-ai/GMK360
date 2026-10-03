import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> AddExpense\(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int\? odometer, Microsoft.AspNetCore.Http.IFormFile\? receiptPhoto\)'
replacement = r'public async Task<IActionResult> AddExpense(int vehicleId, string expenseType, decimal amount, string receiptNumber, string expenseDate, int? odometer, Microsoft.AspNetCore.Http.IFormFile? photo)'
content = re.sub(target, replacement, content)

content = content.replace('if (receiptPhoto != null && receiptPhoto.Length > 0)', 'if (photo != null && photo.Length > 0)')
content = content.replace('photo.CopyToAsync', 'photo.CopyToAsync')
content = content.replace('receiptPhoto.FileName', 'photo.FileName')
content = content.replace('await receiptPhoto.CopyToAsync(stream)', 'await photo.CopyToAsync(stream)')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('AddExpense method parameter renamed to photo.')