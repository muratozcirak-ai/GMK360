import io
import re

filepath = r'GMK360.Web\Controllers\ConstructionProjectController.cs'
with io.open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# I will just write a simple method to drop the methods by using block brace counting if needed, but regex with .*? can work if carefully done.
# Alternatively, I can just replace everything between 'public async Task<IActionResult> ManagePhases' and the next 'public async Task<IActionResult>' with empty.
# Let's find exactly where they are.

import builtins
def remove_method(method_name, content):
    pattern = r'(?:\s*\[[A-Za-z]+\])*\s*public async Task<IActionResult> ' + method_name + r'\(.*?\)\s*\{'
    match = re.search(pattern, content)
    if not match: return content
    
    start_idx = match.start()
    brace_count = 0
    in_method = False
    
    for i in range(match.end() - 1, len(content)):
        if content[i] == '{':
            brace_count += 1
            in_method = True
        elif content[i] == '}':
            brace_count -= 1
            
        if in_method and brace_count == 0:
            end_idx = i + 1
            return content[:start_idx] + content[end_idx:]
    return content

content = remove_method('ManagePhases', content)
content = remove_method('Feasibility', content)
content = remove_method('SaveFeasibility', content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Cleaned Controller")
