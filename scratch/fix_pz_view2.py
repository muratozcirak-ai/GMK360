import codecs
import re

path = 'GMK360.Web/Views/PhaseZero/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Add originalLocation input
target_input = r'(<input type="number" step="0\.01" class="form-control text-end" name="additionalCost" id="modalAddCost">\s*</div>\s*</div>)'
replacement_input = r'''\1
                            <div class="col-md-12 mt-3">
                                <label class="form-label fw-bold text-danger"><i class="bi bi-pin-map-fill"></i> Fiziksel Orijinali Nerede?</label>
                                <input type="text" class="form-control border-danger" name="originalLocation" id="modalOriginalLocation" placeholder="Örn: Ahmet Bey'in çekmecesinde, Muhasebe klasöründe...">
                                <small class="text-muted">Denetimde sorulduğunda veya gerektiğinde ıslak imzalı orijinaline ulaşabilmek için not düşün.</small>
                            </div>'''

content = re.sub(target_input, replacement_input, content)

# Also update the modal javascript function
target_js = r'document\.getElementById\("modalAddCost"\)\.value = data\.additionalCost;'
replacement_js = r'''document.getElementById("modalAddCost").value = data.additionalCost;
                document.getElementById("modalOriginalLocation").value = data.originalLocation || "";'''

content = re.sub(target_js, replacement_js, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)