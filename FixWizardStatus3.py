import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# I want Status == 3 (Tamamlandı) to go to Step 2, and the text should be "Blok/Aşama Tanımla"
content = content.replace('''} else if (status == "3") { // Tamamlanmış (Referans)
                  btnNext.innerHTML = 'Projeyi Kaydet (Detayları Atla) <i class="ph ph-check ms-2"></i>';
                  helpText.innerText = 'Tamamlanmış, arşiv/portföy amacıyla sisteme eklenecek projedir. Blok/aşama detayları atlanır.';''', '''} else if (status == "3") { // Tamamlanmış (Referans)
                  btnNext.innerHTML = 'Blokları ve Katları Tanımla (Bina Yönetimi İçin) <i class="ph ph-arrow-right ms-2"></i>';
                  helpText.innerText = 'Tamamlanmış projedir. Kat ve daireleri girerek Dijital İkiz (Bina Yönetimi) altyapısını kuracaksınız.';''')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
