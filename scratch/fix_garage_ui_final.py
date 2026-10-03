import codecs
import re

path = 'GMK360.Web/Views/CompanyGarage/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# Fix the broken text-end div
bad_html = '''<div class="text-end">
                                <span class="badge bg-@statusColor rounded-pill mb-1">@statusText</span><br/>
                                <span class="badge bg-secondary" title="Gncel Kilometre"><i class="bi bi-speedometer2"></i> @vehicle.CurrentKm KM</span>
                                <span class="badge bg-info text-dark" title="alma Saati ( Makinas)"><i class="bi bi-clock-history"></i> @vehicle.CurrentWorkingHours Saat</span>
                            </div>'''
# In Python, we have to match the encoding properly. I'll just use regex to remove it and inject it into the correct place.

# Let's just rewrite the whole card header with clean UTF-8
pattern = r'<div class="d-flex justify-content-between align-items-start mb-3">.*?</div>\s*</div>\s*<!-- Masraf'
replacement = '''<div class="d-flex justify-content-between align-items-start mb-3">
                            <div class="d-flex align-items-center">
                                <div class="bg-@statusColor bg-opacity-10 text-@statusColor rounded-circle p-3 me-3">
                                    <i class="bi bi-truck fs-3"></i>
                                </div>
                                <div>
                                    <h4 class="fw-bold mb-0 text-dark">@vehicle.PlateNumber</h4>
                                    <span class="text-muted small">@vehicle.BrandModel - @vehicle.VehicleType</span>
                                    <div class="mt-2">
                                        <span class="badge bg-secondary me-1" title="Güncel Kilometre"><i class="bi bi-speedometer2"></i> @vehicle.CurrentKm KM</span>
                                        <span class="badge bg-info text-dark" title="Çalışma Saati (İş Makinası)"><i class="bi bi-clock-history"></i> @vehicle.CurrentWorkingHours Saat</span>
                                    </div>
                                </div>
                            </div>
                            <span class="badge bg-@statusColor rounded-pill">@statusText</span>
                        </div>

                        <!-- Masraf'''

content = re.sub(pattern, replacement, content, flags=re.DOTALL)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)
print('Fixed Index.cshtml layout.')