import codecs

filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ProjectLayout.cshtml'

project_id = '@ViewData["ProjectId"]'

content = f'''@using Microsoft.AspNetCore.Identity
@using GMK360.Core.Entities.Identity
@inject UserManager<ApplicationUser> UserManager
@{{
    Layout = "_Layout";
}}

<div class="container-fluid mt-4">
    <div class="row">
        <!-- Sol Menü -->
        <div class="col-xl-2 col-lg-3 col-md-4 mb-4">
            <div class="card shadow-sm border-0 h-100">
                <div class="list-group list-group-flush" style="font-size: 0.9rem;">
                    <div class="px-3 mb-3 mt-3">
                        <a href="/ConstructionProject/Index" class="btn btn-outline-secondary w-100 rounded-pill fw-bold shadow-sm d-flex justify-content-center align-items-center">
                            <i class="bi bi-arrow-left-circle-fill me-2 fs-5 text-secondary"></i> Tüm Projelere Dön
                        </a>
                    </div>

                    <a href="/ConstructionProject/Details/{project_id}" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "Details" ? "active" : "")">
                        <i class="bi bi-bar-chart-fill me-2 text-primary"></i> Şantiye Panosu
                    </a>
                    <a href="#" class="list-group-item list-group-item-action text-muted" title="Yakında eklenecek">
                        <i class="bi bi-people-fill me-2 text-primary"></i> Puantaj ve Ekipler
                    </a>
                    <a href="/Inventory/Index/{project_id}" class="list-group-item list-group-item-action text-muted" title="Yakında eklenecek">
                        <i class="bi bi-boxes me-2 text-primary"></i> Şantiye Deposu
                    </a>
                    <a href="/ConstructionProject/ManagePhases/{project_id}" class="list-group-item list-group-item-action @(ViewContext.RouteData.Values["Action"]?.ToString() == "ManagePhases" ? "active" : "")">
                        <i class="bi bi-kanban me-2 text-primary"></i> Bütçe ve Fazlar
                    </a>
                    <a href="/ConstructionProject/Details/{project_id}#collapseFaz0" class="list-group-item list-group-item-action">
                        <i class="bi bi-file-earmark-lock-fill me-2 text-danger"></i> Evraklar & Faz 0
                    </a>
                    
                    <div class="list-group-item bg-light fw-bold mt-4 text-uppercase text-center" style="font-size: 0.8rem; color: #6c757d;">
                        Şantiye Modu Aktif
                    </div>
                </div>
            </div>
        </div>

        <!-- Ana İçerik -->
        <div class="col-xl-10 col-lg-9 col-md-8">
            @RenderBody()
        </div>
    </div>
</div>

@section Scripts {{
    @RenderSection("Scripts", required: false)
}}
'''

with codecs.open(filepath, 'w', 'utf-8') as f:
    f.write(content)
