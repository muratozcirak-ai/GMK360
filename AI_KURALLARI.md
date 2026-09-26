# 🚨 GMK360 PROJESİ - AI KODLAMA ANAYASASI 🚨
Bu projede kod yazarken veya güncellerken KESİNLİKLE uyulması gereken kurallar şunlardır:

1. **ASLA DOSYAYI BAŞTAN YAZMA:** Bir Controller, Entity, Interface veya View dosyasında düzeltme istendiğinde, sınıfın tamamını heyecan yapıp sıfırdan oluşturma.

2. **ESKİ METODLARI KORU:** Benden aksine net bir talep gelmediği sürece; dosyada zaten var olan, çalışan GET, POST ve diğer Action metodlarını ASLA SİLME VE EZME.

3. **SADECE FARKI GÖSTER:** Sadece benden istenen düzeltmeyi, güncellemeyi veya yeni eklenen kod bloğunu yaz.

4. **YER TUTUCU KULLAN:** Değiştirmeyeceğin veya müdahale etmeyeceğin mevcut kod bloklarının yerine kodun tamamını yazmak yerine // ... mevcut kodlar korunacak ... yazıp geç.

5. **KONTROLLÜ DOKUNUŞ:** Görevin mevcudu yıkıp yeniden yapmak değil, mevcut mimariye nokta atışı modifikasyon yapmaktır. Her işlemden önce bu kuralları hatırla.

6. **GIT GÜNCELLEMESİ (COMMIT):** Her başarılı işlemden ve çalışan kod yazımından sonra, projenin mevcut sağlam halini güvence altına almak için derhal Git Commit (ve gerekiyorsa Push) yap. Sistem bozulursa en son sağlam noktaya (noktasız bile olsa) kayıpsız dönülebilmelidir.

7. **GET VE POST SİMETRİSİ (UI ODAKLI MANTIK):** GET ve POST metotları yazılırken referans noktası her zaman UI (Arayüz/Görsel) olmalıdır. 
   - GET metodu yazarken UI'da hangi alanlar (input, select) olduğuna bak, o verileri eksiksiz getir.
   - POST metodu yazarken sadece UI'dan gelen verileri yakala ve güncelle.
   - UI'da görünmeyen (Örn: Kim ekledi, Son düzenlenme tarihi vb.) sistem verilerini POST işlemi sırasında "boş geldi" sanıp ezme, sadece değişenleri güncelle.
