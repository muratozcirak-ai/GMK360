import sys

filepath = 'GMK360.Core/Entities/InstitutionContact.cs'

with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

target = "public int InstitutionRecordId { get; set; }"
replacement = """public int AgencyId { get; set; }
        public Agency Agency { get; set; }

        public int InstitutionRecordId { get; set; }"""

if "AgencyId" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("InstitutionContact updated with AgencyId.")
else:
    print("Already updated.")
