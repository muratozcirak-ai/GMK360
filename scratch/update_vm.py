import re
with open('GMK360.Web/Models/CreateProjectWizardViewModel.cs', 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('public string? LayoutPattern { get; set; } // Ortak Baza, BaÄŸÄ±msÄ±z (Tek Temel) vb.', 
                          'public string? LayoutPattern { get; set; }\n        public List<WizardBlockItem> SubBlocks { get; set; } = new List<WizardBlockItem>();')

with open('GMK360.Web/Models/CreateProjectWizardViewModel.cs', 'w', encoding='utf-8') as f:
    f.write(content)
