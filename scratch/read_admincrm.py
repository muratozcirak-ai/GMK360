import io
filepath = r'GMK360.Web\Controllers\AdminCRMController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

in_index = False
for i, line in enumerate(lines):
    if "public async Task<IActionResult> Index" in line:
        in_index = True
    if in_index:
        print(line, end='')
    if in_index and "return View" in line:
        break
