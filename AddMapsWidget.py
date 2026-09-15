import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml'
with codecs.open(filepath, 'r', 'utf-8-sig') as f:
    content = f.read()

maps_widget = '''
<div class="card shadow-sm border-0 rounded-4 mb-4">
    <div class="row g-0">
        <div class="col-md-8 p-4">
            <h5 class="fw-bold text-dark"><i class="bi bi-info-circle text-primary me-2"></i> Şantiye Bilgileri</h5>
            <div class="row mt-3">
                <div class="col-sm-6 mb-3">
                    <small class="text-muted d-block">Aşama / Durum</small>
                    <span class="fw-bold">@Model.Status.ToString().Replace(""_"", "" "")</span>
                </div>
                <div class="col-sm-6 mb-3">
                    <small class="text-muted d-block">Başlangıç Tarihi</small>
                    <span class="fw-bold">@Model.StartDate.ToShortDateString()</span>
                </div>
                <div class="col-12 mb-3">
                    <small class="text-muted d-block">Pazarlama Metni (Vitrin)</small>
                    <span>@(string.IsNullOrEmpty(Model.PublicDescription) ? ""Henüz açıklama girilmedi."" : Model.PublicDescription)</span>
                </div>
            </div>
        </div>
        <div class="col-md-4 bg-light rounded-end-4 position-relative" style="min-height: 200px;">
            @if(!string.IsNullOrEmpty(Model.GoogleMapsUrl))
            {
                <iframe src="@Model.GoogleMapsUrl" width="100%" height="100%" style="border:0; border-radius: 0 1rem 1rem 0; min-height: 200px;" allowfullscreen="""" loading="lazy" referrerpolicy="no-referrer-when-downgrade"></iframe>
                <a href="@Model.GoogleMapsUrl" target="_blank" class="btn btn-sm btn-dark position-absolute bottom-0 end-0 m-3 shadow rounded-pill"><i class="bi bi-geo-alt-fill text-danger me-1"></i> Yol Tarifi Al</a>
            }
            else
            {
                <div class="d-flex flex-column justify-content-center align-items-center h-100 text-muted p-4 text-center">
                    <i class="bi bi-map fs-1 mb-2"></i>
                    <p class="small mb-0">Harita koordinatları girilmemiş. Lojistik ve yol tarifi için projeyi düzenleyip bir Google Maps linki ekleyin.</p>
                </div>
            }
        </div>
    </div>
</div>
'''

content = content.replace('<!-- BARIYER (FAZ 0) UYARISI YERINE GERCEK TABLO -->', maps_widget + '\n\n<!-- BARIYER (FAZ 0) UYARISI YERINE GERCEK TABLO -->')

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
