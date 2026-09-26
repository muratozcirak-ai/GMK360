import sys

filepath = 'GMK360.Web/Controllers/ModuleDocumentRuleController.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

replacement = """        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            ModelState.Remove("SystemLegalDocumentTemplate");
            ModelState.Remove("PrerequisiteTemplate");
            
            if (ModelState.IsValid)
            {
                if (PrerequisiteTemplateIdsList != null && PrerequisiteTemplateIdsList.Length > 0)
                {
                    var validIds = PrerequisiteTemplateIdsList.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                    if (validIds.Any())
                    {
                        model.PrerequisiteTemplateIds = string.Join(",", validIds);
                    }
                    else
                    {
                        model.PrerequisiteTemplateIds = null;
                    }
                }"""

content = content.replace("""        [HttpPost]
        public async Task<IActionResult> Create(ModuleDocumentRule model, string[] PrerequisiteTemplateIdsList)
        {
            ModelState.Remove("SystemLegalDocumentTemplate");
            ModelState.Remove("PrerequisiteTemplate");
            
            if (ModelState.IsValid)
            {
                if (PrerequisiteTemplateIdsList != null && PrerequisiteTemplateIdsList.Length > 0)
                {
                    model.PrerequisiteTemplateIds = string.Join(",", PrerequisiteTemplateIdsList);
                }""", replacement)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
