import io

filepath = r'GMK360.Web\Views\ProjectFinance\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

header_card = """
    <!-- Proje Kimliği (Genel Bilgiler) -->
    <div class="card shadow-sm border-0 mb-4 rounded-4 bg-light">
        <div class="card-body p-4">
            <div class="row align-items-center">
                <div class="col-md-6 border-end">
                    <h4 class="fw-bold text-dark mb-1"><i class="bi bi-building me-2 text-primary"></i> @ViewData["ProjectName"]</h4>
                    <p class="text-muted mb-0"><i class="bi bi-geo-alt me-1"></i> @ViewBag.Address</p>
                </div>
                <div class="col-md-6">
                    <div class="row text-center">
                        <div class="col-4">
                            <h6 class="text-muted mb-1" style="font-size:0.85rem;">Başlama Tarihi</h6>
                            <span class="fw-bold text-dark">@ViewBag.StartDate</span>
                        </div>
                        <div class="col-4 border-start border-end">
                            <h6 class="text-muted mb-1" style="font-size:0.85rem;">Tahmini Teslim</h6>
                            <span class="fw-bold text-dark">@ViewBag.EndDate</span>
                        </div>
                        <div class="col-4">
                            <h6 class="text-muted mb-1" style="font-size:0.85rem;">Süre</h6>
                            <span class="fw-bold text-primary">@ViewBag.Duration</span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
"""

content = content.replace('<div class="container-fluid mt-2">\n    \n    <!--', '<div class="container-fluid mt-2">\n    ' + header_card + '\n    <!--')

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
