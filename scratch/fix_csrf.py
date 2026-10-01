import io
import re

filepath = r'GMK360.Web\Controllers\ProjectFinanceController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add [IgnoreAntiforgeryToken] to the endpoint
content = content.replace('[HttpPost("ProjectFinance/UpdateExpectations")]\n        public async Task<IActionResult> UpdateExpectations', '[IgnoreAntiforgeryToken]\n        [HttpPost("ProjectFinance/UpdateExpectations")]\n        public async Task<IActionResult> UpdateExpectations')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Added IgnoreAntiforgeryToken.")
