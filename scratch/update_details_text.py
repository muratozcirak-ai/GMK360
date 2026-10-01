import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the text inside the inviteManagerModal
old_text_pattern = r'<p class="text-muted small mb-4">Bu davet, bina yöneticisine "Teklifleri tek ekranda görebileceği".*?\(GMK360 Truva Atı\)\.</p>'
new_text = r'<p class="text-muted small mb-4">Müşterinize (veya bina yönetimine) <strong>"Şeffaf ve Modern İnşaat"</strong> vizyonunuzu gösterin. Bu link ile sisteme davet ettiğiniz müşteri; evinin şantiye aşamalarını evden takip edebilir, parke/boya/seramik gibi iç detay seçimlerini dijital olarak yapabilir. <em>(Müşteriyi sisteme bağlama stratejisi)</em></p>'

content = re.sub(old_text_pattern, new_text, content, flags=re.DOTALL)

# Update the Card Title slightly
content = content.replace("Müşteriyi Davet Et", "Müşteri Portalına Davet")
content = content.replace("Bina Yönetim Portalı", "Müşteri Dijital Ev Portalı") # In case it was there

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated Details.cshtml texts")
