import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\SubcontractorContract\Index.cshtml'

with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

pattern = r'@if \(\!Model\.Any\(\)\)\s*\{\s*<div class="text-center py-5 text-muted">\s*<i class="bi bi-journal-x fs-1 d-block mb-3"></i>\s*.*?</div>\s*\}'

replacement = '''@if (!Model.Any())
            {
                <div class="text-center py-5">
                    <div class="d-inline-flex align-items-center justify-content-center bg-light rounded-circle mb-3" style="width: 80px; height: 80px;">
                        <i class="bi bi-file-earmark-text text-secondary fs-1"></i>
                    </div>
                    <h5 class="fw-bold text-dark">Henüz Taşeron Sözleşmesi Eklenmemiş</h5>
                    <p class="text-muted mx-auto" style="max-width: 500px;">
                        Hakediş işlemlerine başlayabilmek için öncelikle bir taşeron sözleşmesi oluşturmanız gerekmektedir.
                        <br><br>
                        Sözleşmenizi ekledikten sonra, listedeki sözleşme satırına tıklayarak <strong>Hakediş (Ödeme) Geçmişini</strong> görüntüleyebilir ve esnek hakediş girişleri yapabilirsiniz.
                    </p>
                    <a href="/SubcontractorContract/Create" class="btn btn-outline-primary mt-2">
                        <i class="bi bi-plus-lg me-1"></i> İlk Sözleşmeyi Oluştur
                    </a>
                </div>
            }'''

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
