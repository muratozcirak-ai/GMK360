import codecs
import re

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_Layout.cshtml'
with codecs.open(filepath, 'r', 'utf-8', errors='ignore') as f:
    content = f.read()

logo_target = '''<span class="fs-3 fw-bold text-navy" style="letter-spacing:-1px;">GMK<span class="text-orange">360</span></span>
            </a>'''

logo_replace = '''<span class="fs-3 fw-bold text-navy" style="letter-spacing:-1px;">GMK<span class="text-orange">360</span></span>
            </a>
            @if(ViewData["ProjectName"] != null)
            {
                <span class="fs-3 text-muted mx-3">|</span>
                <i class="bi bi-building-check fs-4 text-primary me-2"></i>
                <span class="fw-bold fs-4 text-uppercase text-dark" style="letter-spacing: 0px;">@ViewData["ProjectName"] ŞANTİYESİ</span>
            }'''

content = content.replace(logo_target, logo_replace)

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
