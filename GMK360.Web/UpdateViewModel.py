import re

with open(r"Models\CreateProjectWizardViewModel.cs", "r", encoding="utf-8") as f:
    content = f.read()

find_str = """        public string? StructureType { get; set; } // independent, podium, tower
        public string? ParentIndex { get; set; } // If tower, index of the parent podium"""

replace_str = """        public string? StructureType { get; set; } // independent, podium, tower
        public string? ParentIndex { get; set; } // If tower, index of the parent podium
        
        public string? LayoutPattern { get; set; } // Ayrık Nizam, Bitişik Nizam vs.
        public string? AttachedToBlock { get; set; } // Hangi bloğa bitişik"""

content = content.replace(find_str, replace_str)

with open(r"Models\CreateProjectWizardViewModel.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Updated WizardBlockItem")
