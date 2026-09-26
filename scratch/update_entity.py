import sys

filepath = 'GMK360.Core/Entities/Construction/ProjectLegalDocument.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "public string? IssueNotes { get; set; }"
replacement = """public string? IssueNotes { get; set; }

        public decimal? DocumentFee { get; set; }
        public decimal? AdditionalCost { get; set; }"""

if "DocumentFee" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("Entity updated.")
else:
    print("Entity already updated.")
