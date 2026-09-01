import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add missing using statements if not present
if "using Microsoft.AspNetCore.Hosting;" not in content:
    content = content.replace("using Microsoft.AspNetCore.Mvc;", "using Microsoft.AspNetCore.Mvc;\nusing Microsoft.AspNetCore.Hosting;\nusing System.IO;\nusing Microsoft.AspNetCore.Http;")

# Inject IWebHostEnvironment
if "IWebHostEnvironment" not in content:
    content = content.replace("public ConstructionProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)", "private readonly IWebHostEnvironment _hostEnvironment;\n\n        public ConstructionProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)")
    content = content.replace("_userManager = userManager;", "_userManager = userManager;\n            _hostEnvironment = hostEnvironment;")

new_method = """        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWizard([FromBody] GMK360.Web.Models.CreateProjectWizardViewModel model)
        {
            // FromBody is wrong because it's multipart form data. We must remove FromBody for IFormFile to work.
            // Wait, JS sends it as JSON if it's wizard, or FormData if it's updated.
"""
