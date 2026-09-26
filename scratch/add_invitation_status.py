import re

filepath = r'GMK360.Core\Entities\B2b\B2bCompany.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

enum_to_add = """
    public enum B2bInvitationStatus
    {
        Shadow = 0,      // 🌑 Gölge (Davet Edilmedi)
        Invited = 1,     // 📨 Davet Gönderildi
        Active = 2,      // 🟢 Aktif (Sisteme Girdi)
        Pro = 3          // 👑 Pro Kullanıcı
    }
"""

field_to_add = "public B2bInvitationStatus InvitationStatus { get; set; } = B2bInvitationStatus.Shadow;"

if "B2bInvitationStatus" not in content:
    # Insert Enum
    content = content.replace("public enum LegalEntityType", enum_to_add + "\n    public enum LegalEntityType")
    # Insert Field
    content = content.replace("public bool IsVerified { get; set; } = false;", "public bool IsVerified { get; set; } = false;\n        public B2bInvitationStatus InvitationStatus { get; set; } = B2bInvitationStatus.Shadow;")
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added InvitationStatus to B2bCompany")
else:
    print("Already added.")
