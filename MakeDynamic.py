import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

# Replace the button
old_button = '<button type="submit" class="btn btn-primary px-5"><i class="bi bi-check2-circle me-1"></i> Şantiyeyi Oluştur</button>'
new_button = '<button type="submit" id="submitBtn" class="btn btn-primary px-5"><i class="bi bi-check2-circle me-1"></i> Şantiyeyi Oluştur</button>'
content = content.replace(old_button, new_button)

# Add Javascript to change texts dynamically
js_code = '''
@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
    <script>
        document.addEventListener("DOMContentLoaded", function() {
            const statusSelect = document.getElementById("Status");
            const submitBtn = document.getElementById("submitBtn");
            const formTitle = document.getElementById("formTitle");
            const formDesc = document.getElementById("formDesc");

            function updateUI() {
                const val = statusSelect.value;
                if (val === "1") { // Teklif
                    submitBtn.innerHTML = '<i class="bi bi-folder-plus me-1"></i> Projeyi (Aday) Kaydet';
                    submitBtn.className = 'btn btn-info text-white px-5';
                    formTitle.innerHTML = '<i class="bi bi-file-earmark-text me-2"></i>Yeni Proje (Teklif) Ekle';
                    formDesc.innerText = 'Henüz anlaşması yapılmamış, görüşme aşamasındaki projenizi kaydedin.';
                } 
                else if (val === "2") { // Aktif
                    submitBtn.innerHTML = '<i class="bi bi-cone-striped me-1"></i> Şantiyeyi Başlat';
                    submitBtn.className = 'btn btn-primary px-5';
                    formTitle.innerHTML = '<i class="bi bi-building-add me-2"></i>Yeni Şantiye Başlat';
                    formDesc.innerText = 'Anlaşması tamamlanmış ve çivisi çakılacak aktif şantiyenizi başlatın.';
                }
                else if (val === "3") { // Tamamlandı
                    submitBtn.innerHTML = '<i class="bi bi-archive me-1"></i> Geçmiş Projeyi Arşive Ekle';
                    submitBtn.className = 'btn btn-secondary px-5';
                    formTitle.innerHTML = '<i class="bi bi-trophy me-2"></i>Tamamlanan Proje Ekle';
                    formDesc.innerText = 'Geçmişte bitirdiğiniz projeyi sisteme ekleyerek referanslarınıza ve dijital ikize aktarın.';
                }
                else if (val === "4") { // Satışta
                    submitBtn.innerHTML = '<i class="bi bi-tags me-1"></i> Satış Projesi Olarak Kaydet';
                    submitBtn.className = 'btn btn-warning text-dark px-5';
                    formTitle.innerHTML = '<i class="bi bi-house-door me-2"></i>Satış / Topraktan Proje';
                    formDesc.innerText = 'Topraktan veya hazır satışta olan gayrimenkul projenizi kaydedin.';
                }
            }

            statusSelect.addEventListener("change", updateUI);
            updateUI(); // initial load
        });
    </script>
}
'''

# We also need to add id="formTitle" and id="formDesc" to the headers
content = content.replace('<h4 class="mb-0 fw-bold text-primary"><i class="bi bi-building-add me-2"></i>Yeni Şantiye / Proje Başlat</h4>', '<h4 id="formTitle" class="mb-0 fw-bold text-primary"><i class="bi bi-building-add me-2"></i>Yeni Şantiye / Proje Başlat</h4>')
content = content.replace('<p class="text-muted small mb-0 mt-1">Eski veya yeni bir şantiyenizi hızlıca sisteme ekleyin. İsterseniz detayları (harita, bloklar vs.) daha sonra proje ayarlarından doldurabilirsiniz.</p>', '<p id="formDesc" class="text-muted small mb-0 mt-1">Eski veya yeni bir şantiyenizi hızlıca sisteme ekleyin. İsterseniz detayları (harita, bloklar vs.) daha sonra proje ayarlarından doldurabilirsiniz.</p>')

# Replace Scripts section
content = re.sub(r'@section Scripts \{.*?\}', js_code, content, flags=re.DOTALL)


with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
