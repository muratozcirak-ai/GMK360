import sys

filepath = 'GMK360.Core/Entities/Construction/ProjectLegalDocument.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "public decimal? AdditionalCost { get; set; }"
replacement = """public decimal? AdditionalCost { get; set; }

        public int? TargetInstitutionId { get; set; }
        public InstitutionRecord TargetInstitution { get; set; }

        public int? TargetContactId { get; set; }
        public InstitutionContact TargetContact { get; set; }"""

if "TargetInstitutionId" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("ProjectLegalDocument updated.")
else:
    print("Already updated.")
