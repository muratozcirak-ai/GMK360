import codecs

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

target = '''<div class="col-12">
                                      <label class="form-label fw-bold">Masraf Türü</label>'''

replacement = '''<div class="col-12">
                                      <label class="form-label fw-bold text-dark">Fiş Anındaki Kilometre <span class="text-muted fw-normal">(İsteğe Bağlı)</span></label>
                                      <input type="number" name="odometer" class="form-control" placeholder="Örn: 15400" />
                                      <small class="text-muted d-block mt-1" style="font-size:0.75rem;">Yakıt fişi kesildiğinde aracın KM'si (Yakıt tüketimi hesaplamak için idealdir)</small>
                                  </div>
                                  <div class="col-12">
                                      <label class="form-label fw-bold">Masraf Türü</label>'''

content = content.replace(target, replacement)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Updated AddExpense UI')