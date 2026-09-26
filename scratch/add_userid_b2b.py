import re

filepath = r'GMK360.Core\Entities\B2b\B2bCompany.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Add AddedByUserId (string) next to AddedByAgencyId
if "public string? AddedByUserId" not in content:
    content = content.replace("public int? AddedByAgencyId { get; set; }", "public string? AddedByUserId { get; set; }\n        public Identity.ApplicationUser? AddedByUser { get; set; }\n        \n        public int? AddedByAgencyId { get; set; }")

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
