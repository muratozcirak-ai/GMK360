import codecs
import re
import glob

views = glob.glob('GMK360.Web/Views/Phase*/Index.cshtml')

for path in views:
    try:
        with codecs.open(path, 'r', 'utf-8-sig') as f:
            content = f.read()

        # Update the header total span
        # <span class="text-success">@catTotal.ToString("N2") ?</span>
        content = re.sub(
            r'<span class="text-success">@catTotal\.ToString\("N2"\).*?</span>',
            r'<span class="badge bg-white text-primary border border-primary rounded-pill px-3 py-1 shadow-sm fs-6">Toplam: @catTotal.ToString("N2") ₺</span>',
            content
        )

        # Fix currency in Modal Label
        content = re.sub(
            r'Tahmini Toplam Tutar \(\?\)',
            r'Tahmini Toplam Tutar (₺)',
            content
        )

        # Fix currency in input group addon
        content = re.sub(
            r'<span class="input-group-text bg-primary text-white border-primary">\?</span>',
            r'<span class="input-group-text bg-primary text-white border-primary">₺</span>',
            content
        )
        
        # Also fix the accordion icon layout: <i class="bi bi-diagram-2 text-primary me-2"></i> @cat
        content = re.sub(
            r'<span><i class="bi bi-diagram-2 text-primary me-2"></i> @cat</span>',
            r'<span class="fw-bold fs-5 text-dark"><i class="bi bi-diagram-2 text-primary me-2"></i> @cat</span>',
            content
        )

        with codecs.open(path, 'w', 'utf-8-sig') as f:
            f.write(content)
            
        print(f"Updated {path}")
    except Exception as e:
        print(f"Error processing {path}: {e}")
