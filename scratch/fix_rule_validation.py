import sys

filepath = 'GMK360.Web/Controllers/ModuleDocumentRuleController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            ModelState.Remove("SystemLegalDocumentTemplate");
            ModelState.Remove("PrerequisiteTemplate");
            
            if (ModelState.IsValid)"""

content = content.replace("""        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            if (ModelState.IsValid)""", replacement)

with open(filepath, 'w', encoding='utf-8-sig') as f:
    f.write(content)
