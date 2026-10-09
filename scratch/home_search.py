import codecs

path = 'GMK360.Web/Views/Home/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

search_bar = '''
        <!-- AKILLI ARAMA MOTORU (EMLAK & USTA) -->
        <div class="row justify-content-center mt-5">
            <div class="col-lg-8">
                <div class="bg-white p-2 rounded-pill shadow-lg d-flex align-items-center">
                    <select class="form-select border-0 bg-transparent fw-bold text-navy ms-3" style="width: auto; box-shadow: none;">
                        <option>Satılık Konut</option>
                        <option>Kiralık Konut</option>
                        <option>Günlük Kiralık</option>
                        <option>Bölgede Usta Bul</option>
                    </select>
                    <div class="vr mx-2 text-muted"></div>
                    <input type="text" class="form-control border-0 bg-transparent shadow-none px-3" placeholder="İl, ilçe, proje adı veya usta mesleği...">
                    <button class="btn btn-orange rounded-pill px-5 py-2 ms-2 fw-bold text-white"><i class="ph-fill ph-magnifying-glass me-2"></i> Ara</button>
                </div>
            </div>
        </div>
'''

if 'AKILLI ARAMA MOTORU' not in content:
    content = content.replace('<div class="d-flex justify-content-center gap-3">', search_bar + '\n        <div class="d-flex justify-content-center gap-3 mt-5">')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)