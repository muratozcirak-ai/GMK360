import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\_ProjectCardPartial.cshtml'

content = '''@model GMK360.Core.Entities.Construction.ConstructionProject
@using GMK360.Core.Entities.Construction

@{
    var imgUrl = !string.IsNullOrEmpty(Model.CoverImageUrl) ? Model.CoverImageUrl : "/img/default-project.jpg";
    var statusText = Model.Status switch {
        ProjectStatus.Projelendirme_Teklif => "Teklif / Planlama",
        ProjectStatus.Aktif_Santiye => "Aktif Şantiye",
        ProjectStatus.Satista_Topraktan => "Satışta (Topraktan)",
        ProjectStatus.Tamamlandi_Teslim => "Tamamlandı",
        _ => "Bilinmiyor"
    };
    
    var statusColor = Model.Status switch {
        ProjectStatus.Projelendirme_Teklif => "bg-secondary",
        ProjectStatus.Aktif_Santiye => "bg-primary",
        ProjectStatus.Satista_Topraktan => "bg-warning text-dark",
        ProjectStatus.Tamamlandi_Teslim => "bg-success",
        _ => "bg-dark"
    };

    // İlerleme simülasyonu (gerçek veriye bağlanabilir)
    int progress = Model.Status == ProjectStatus.Tamamlandi_Teslim ? 100 : (Model.Status == ProjectStatus.Aktif_Santiye ? 45 : 0);
}

<div class="card h-100 border-0 shadow-sm rounded-4 overflow-hidden" style="transition: transform 0.2s; cursor:pointer;" onmouseover="this.style.transform='translateY(-5px)'" onmouseout="this.style.transform='translateY(0)'" onclick="location.href='/ConstructionProject/Dashboard/@Model.Id'">
    
    <!-- Proje Görseli -->
    <div class="position-relative" style="height: 180px;">
        <img src="@imgUrl" class="w-100 h-100 object-fit-cover" alt="@Model.Name" onerror="this.src='/img/bg1.jpg'">
        <div class="position-absolute top-0 start-0 w-100 h-100 bg-dark bg-opacity-25"></div>
        <div class="position-absolute top-0 end-0 p-2">
            <span class="badge @statusColor shadow-sm rounded-pill px-3 py-2"><i class="bi bi-circle-fill small me-1"></i> @statusText</span>
        </div>
    </div>

    <div class="card-body p-4 d-flex flex-column">
        <h5 class="fw-bold text-dark text-truncate mb-1" title="@Model.Name">@Model.Name</h5>
        <p class="text-muted small mb-3"><i class="bi bi-geo-alt-fill text-danger me-1"></i> @(string.IsNullOrEmpty(Model.Address) ? "Konum Belirtilmedi" : Model.Address)</p>
        
        <div class="mt-auto">
            <div class="d-flex justify-content-between small text-muted mb-1">
                <span>Başlangıç</span>
                <span class="fw-bold text-dark">@Model.StartDate.ToString("MMM yyyy")</span>
            </div>
            @if(Model.Status == ProjectStatus.Aktif_Santiye || Model.Status == ProjectStatus.Tamamlandi_Teslim)
            {
                <div class="d-flex justify-content-between small text-muted mb-2">
                    <span>Tamamlanma</span>
                    <span class="fw-bold text-success">@progress%</span>
                </div>
                <div class="progress" style="height: 6px;">
                    <div class="progress-bar @statusColor" role="progressbar" style="width: @progress%" aria-valuenow="@progress" aria-valuemin="0" aria-valuemax="100"></div>
                </div>
            }
        </div>
    </div>
    <div class="card-footer bg-white border-top-0 p-3 pt-0 text-center">
        <a href="/ConstructionProject/Dashboard/@Model.Id" class="btn btn-outline-primary btn-sm rounded-pill w-100 fw-bold">Şantiye Panosuna Git <i class="bi bi-arrow-right"></i></a>
    </div>
</div>
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
