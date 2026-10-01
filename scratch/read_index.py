import io
filepath = r'GMK360.Web\Controllers\PhaseZeroController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

start = 0
for i, line in enumerate(lines):
    if "public async Task<IActionResult> Index(int projectId)" in line:
        start = i
        break

print("".join(lines[start:start+30]))
