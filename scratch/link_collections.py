import sys

filepath = 'GMK360.Core/Entities/Construction/AgencyPhonebook.cs'

with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

target = "public string? LinkedUserId { get; set; }"
replacement = """public string? LinkedUserId { get; set; }

        public virtual System.Collections.Generic.ICollection<AgencyPhonebookBranch> Branches { get; set; } = new System.Collections.Generic.List<AgencyPhonebookBranch>();
        public virtual System.Collections.Generic.ICollection<AgencyPhonebookContact> Contacts { get; set; } = new System.Collections.Generic.List<AgencyPhonebookContact>();"""

if "ICollection<AgencyPhonebookBranch>" not in content:
    content = content.replace(target, replacement)
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
        print("AgencyPhonebook updated with Collections.")
else:
    print("Already updated.")
