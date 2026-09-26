import re

filepath = r'GMK360.Core\Entities\B2b\B2bCompany.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace('public ICollection<B2bDocument> Documents { get; set; } = new List<B2bDocument>();', '')

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
