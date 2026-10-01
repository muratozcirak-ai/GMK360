import io
import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Remove .Include(p => p.Phases) and its .ThenInclude chains
content = re.sub(r'\.Include\([A-Za-z0-9_]+\s*=>\s*[A-Za-z0-9_]+\.Phases\)(?:\s*\.ThenInclude\([A-Za-z0-9_]+\s*=>\s*[A-Za-z0-9_]+\.[A-Za-z0-9_]+\))*', '', content)

# Remove completely obsolete methods
def remove_method(method_name, text):
    pattern = r'(?:\[.*?\]\s*)*public\s+(?:async\s+)?(?:Task<IActionResult>|IActionResult|Task<JsonResult>|JsonResult|Task<bool>|bool)\s+' + method_name + r'\s*\('
    match = re.search(pattern, text)
    if not match: return text
    
    start = match.start()
    brace_idx = text.find('{', match.end())
    if brace_idx == -1: return text
    
    brace_count = 1
    end_idx = brace_idx + 1
    while brace_count > 0 and end_idx < len(text):
        if text[end_idx] == '{': brace_count += 1
        elif text[end_idx] == '}': brace_count -= 1
        end_idx += 1
    return text[:start] + text[end_idx:]

obsolete = [
    'DraftPhaseDetails', 'AddPhaseMessage', 'ApproveDraftPhase', 'AddPhaseTask', 
    'GetTaskDetails', 'AddTaskMessage', 'AddTaskCost', 'GetPhaseDiary', 
    'AddPhaseDiaryMessage', 'AddPhase'
]
for m in obsolete:
    content = remove_method(m, content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Cleaned Includes and Methods")
