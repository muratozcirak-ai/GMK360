import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix duplicate attributes on CreateWizard
content = content.replace("""        [HttpPost]
        [ValidateAntiForgeryToken]
        
        // WIZARD CREATE ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard""", """        // WIZARD CREATE ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard""")

# The original Create method now lacks its attributes, let's restore them:
content = content.replace("""        public async Task<IActionResult> Create([Bind(""", """        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(""")

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed duplicate attributes.")
