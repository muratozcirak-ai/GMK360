import io
import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Instead of parsing everything, I'll remove any method that contains these strings:
# "PhaseTasks", "TaskMessages", "TaskCosts", "PhaseMessages", "PhaseApprovals", "ProjectPhases", "TaskDocument"
keywords = ['PhaseTasks', 'TaskMessages', 'TaskCosts', 'PhaseMessages', 'PhaseApprovals', 'ProjectPhases', 'TaskDocument', 'PhaseWorkerDemands', 'Phases']

# We need a robust way to delete methods containing these.
# Let's write a simple C# parser that extracts methods and filters them.

new_content = ""
idx = 0

def find_methods(text):
    methods = []
    # Match: public [async Task<...>] MethodName(...)
    pattern = r'(?:\[.*?\]\s*)*public\s+(?:async\s+)?(?:Task<IActionResult>|IActionResult|Task<JsonResult>|JsonResult|Task<bool>|bool)\s+([A-Za-z0-9_]+)\s*\('
    for match in re.finditer(pattern, text):
        start = match.start()
        # Find {
        brace_idx = text.find('{', match.end())
        if brace_idx == -1: continue
        
        brace_count = 1
        end_idx = brace_idx + 1
        while brace_count > 0 and end_idx < len(text):
            if text[end_idx] == '{': brace_count += 1
            elif text[end_idx] == '}': brace_count -= 1
            end_idx += 1
        
        methods.append((start, end_idx, text[start:end_idx]))
    return methods

methods = find_methods(content)
to_remove = []
for start, end, body in methods:
    if any(kw in body for kw in keywords):
        # ensure it's not a generic method like Details. We should only remove if it's explicitly querying DbContext with these keywords
        if 'await _context.' in body or '_context.' in body or 'ConstructionProject' in body:
            to_remove.append((start, end))

new_content = ""
last_idx = 0
for start, end in to_remove:
    # check if 'Details' is in it, we DO NOT WANT to delete Details!
    # Wait, Details might use .Phases ? Let's check manually.
    pass

print(len(to_remove), "methods identified.")
