import sys

filepath = 'GMK360.Core/Entities/SystemLegalDocumentTemplate.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "public GMK360.Core.Enums.InstitutionCategory? Category { get; set; }"
replacement = "public string? Category { get; set; }"

content = content.replace(target, replacement)
with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
