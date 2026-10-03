import codecs
import re

path = 'GMK360.Web/Controllers/ConstructionProjectExpensesController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = r'public async Task<IActionResult> Create\(int ProjectId, int ExpenseType, string Title, string Description, decimal Amount, string DocumentNo, DateTime ExpenseDate, bool IsPaid\)\s*\{'
replacement = '''public async Task<IActionResult> Create(int? ProjectId, int ExpenseType, string Title, string Description, decimal Amount, string DocumentNo, DateTime ExpenseDate, bool IsPaid, Microsoft.AspNetCore.Http.IFormFile photo)
        {
            string photoPath = null;
            if (photo != null && photo.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(photo.FileName);
                var filePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
                photoPath = "/uploads/" + fileName;
            }'''

content = re.sub(target, replacement, content)

# Also update the object creation
target2 = r'ProjectId = ProjectId,'
replacement2 = '''ProjectId = ProjectId,
                PhotoPath = photoPath,'''
content = content.replace(target2, replacement2)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Controller Create updated.')