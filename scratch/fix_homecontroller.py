import re

with open(r'GMK360.Web\Controllers\HomeController.cs', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# We need to insert Faq and Legal before the last closing brace of the class.
# The class probably ends with }
# Let's find the last } and insert our methods there.

methods_to_add = '''
    public IActionResult Faq()
    {
        return View();
    }

    public IActionResult Legal(string doc)
    {
        ViewBag.DocType = doc;
        return View();
    }
'''

# Find the last occurrence of }
last_brace_index = content.rfind('}')
if last_brace_index != -1:
    content = content[:last_brace_index] + methods_to_add + content[last_brace_index:]

with open(r'GMK360.Web\Controllers\HomeController.cs', 'w', encoding='utf-8-sig') as f:
    f.write(content)
