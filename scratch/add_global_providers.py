import sys

filepath = 'GMK360.Web/Controllers/AdminController.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "public IActionResult Users()"
replacement = """public IActionResult GlobalProviders()
        {
            return View();
        }

        public IActionResult Users()"""

if "public IActionResult GlobalProviders()" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AdminController updated.")
else:
    print("Already updated.")
