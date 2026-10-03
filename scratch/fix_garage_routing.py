import codecs
import re

path = 'GMK360.Web/Controllers/CompanyGarageController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix the duplicate [HttpGet] on AllExpenses
content = re.sub(r'\[HttpGet\]\s*\[HttpGet\("CompanyGarage/AllExpenses"\)\]', r'[HttpGet("CompanyGarage/AllExpenses")]', content)

# Restore [HttpGet] on VehicleExpenses
content = re.sub(r'(\s+)public async Task<IActionResult> VehicleExpenses\(int id\)', r'\1[HttpGet]\1public async Task<IActionResult> VehicleExpenses(int id)', content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed routing issue.')