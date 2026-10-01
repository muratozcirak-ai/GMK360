import io
import re

filepath = r'GMK360.Web\Controllers\BuildingManagementController.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace success message for customer
old_msg = "Tebrikler! Bina profilinizi başarıyla devraldınız. Diğer inşaat firmalarından gelecek teklifleri artık tek havuzda toplayıp yönetebilirsiniz."
new_msg = "Tebrikler! Müşteri profiliniz oluşturuldu. Artık projenizin şantiye aşamalarını takip edebilir ve iç mekan malzeme seçimlerinizi yapabilirsiniz."
content = content.replace(old_msg, new_msg)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated controller success message")
