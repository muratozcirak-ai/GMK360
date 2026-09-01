import re

filepath = r"C:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs"
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Fix constructor
content = re.sub(r'public ConstructionProjectController\(ApplicationDbContext context, UserManager<ApplicationUser> userManager\)\s*\{\s*_context = context;\s*_userManager = userManager;\s*\}', 
"""private readonly IWebHostEnvironment _hostEnvironment;

        public ConstructionProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _hostEnvironment = hostEnvironment;
        }""", content)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Fixed Constructor.")
