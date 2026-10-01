import io
import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

def find_methods(text):
    methods = []
    pattern = r'(?:\[.*?\]\s*)*public\s+(?:async\s+)?(?:Task<IActionResult>|IActionResult|Task<JsonResult>|JsonResult|Task<bool>|bool)\s+([A-Za-z0-9_]+)\s*\('
    for match in re.finditer(pattern, text):
        start = match.start()
        brace_idx = text.find('{', match.end())
        if brace_idx == -1: continue
        brace_count = 1
        end_idx = brace_idx + 1
        while brace_count > 0 and end_idx < len(text):
            if text[end_idx] == '{': brace_count += 1
            elif text[end_idx] == '}': brace_count -= 1
            end_idx += 1
        methods.append((start, end_idx, match.group(1), text[start:end_idx]))
    return methods

methods = find_methods(content)
keywords = ['PhaseTasks', 'TaskMessages', 'TaskCosts', 'PhaseMessages', 'PhaseApprovals', 'ProjectPhases', 'TaskDocument', 'PhaseWorkerDemands', '.Phases']

for start, end, name, body in methods:
    if any(kw in body for kw in keywords):
        print(name)
