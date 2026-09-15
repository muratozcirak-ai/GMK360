import codecs

# We will completely replace Create.cshtml with a simple form.
filepath = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml'

new_view = '''@model GMK360.Core.Entities.Construction.ConstructionProject

@{
    ViewData["Title"] = "Yeni Şantiye (Hızlı Giriş)";
    Layout = "~/Views/Shared/_ConstructionLayout.cshtml";
}

<div class="row justify-content-center">
    <div class="col-md-8">
        <div class="card shadow-sm border-0 rounded-4">
            <div class="card-header bg-white border-bottom py-3">
                <h4 class="mb-0 fw-bold text-primary"><i class="bi bi-building-add me-2"></i>Yeni Şantiye / Proje Başlat</h4>
                <p class="text-muted small mb-0 mt-1">Eski veya yeni bir şantiyenizi hızlıca sisteme ekleyin. İsterseniz detayları (harita, bloklar vs.) daha sonra proje ayarlarından doldurabilirsiniz.</p>
            </div>
            <div class="card-body p-4">
                <form asp-action="CreateSimple" method="post">
                    <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

                    <div class="mb-3">
                        <label asp-for="Name" class="form-label fw-bold">Şantiye / Proje Adı <span class="text-danger">*</span></label>
                        <input asp-for="Name" class="form-label form-control form-control-lg" placeholder="Örn: Yaşar Bey Villaları, GMK Plaza..." required />
                        <span asp-validation-for="Name" class="text-danger"></span>
                    </div>

                    <div class="mb-3">
                        <label asp-for="Address" class="form-label fw-bold">Açık Adres <span class="text-danger">*</span></label>
                        <textarea asp-for="Address" class="form-control" rows="2" placeholder="Şantiyenin bulunduğu açık adres..." required></textarea>
                        <span asp-validation-for="Address" class="text-danger"></span>
                    </div>

                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label asp-for="StartDate" class="form-label fw-bold">Başlangıç Tarihi</label>
                            <input asp-for="StartDate" type="date" class="form-control" value="@DateTime.Now.ToString("yyyy-MM-dd")" />
                        </div>
                        <div class="col-md-6">
                            <label asp-for="Status" class="form-label fw-bold">Proje Durumu</label>
                            <select asp-for="Status" class="form-select">
                                <option value="1">Projelendirme / Teklif Aşamasında</option>
                                <option value="2" selected>Aktif Şantiye (Devam Ediyor)</option>
                                <option value="3">Tamamlandı (Eski / Teslim Edildi)</option>
                                <option value="4">Satışta / Topraktan</option>
                            </select>
                        </div>
                    </div>

                    <hr class="my-4" />
                    <h5 class="fw-bold mb-3"><i class="bi bi-info-circle text-muted me-2"></i>Yapı Özellikleri <small class="text-muted fs-6 fw-normal">(İsteğe Bağlı)</small></h5>

                    <div class="row">
                        <div class="col-md-4 mb-3">
                            <label asp-for="TotalFloors" class="form-label text-muted">Kaç Katlı?</label>
                            <input asp-for="TotalFloors" type="number" class="form-control" placeholder="Örn: 5" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <label asp-for="TargetTotalApartments" class="form-label text-muted">Daire Sayısı</label>
                            <input asp-for="TargetTotalApartments" type="number" class="form-control" placeholder="Örn: 20" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <label asp-for="TargetTotalShops" class="form-label text-muted">Dükkan Sayısı</label>
                            <input asp-for="TargetTotalShops" type="number" class="form-control" placeholder="Örn: 2" />
                        </div>
                    </div>

                    <div class="mt-4 d-grid gap-2 d-md-flex justify-content-md-end">
                        <a asp-action="Index" class="btn btn-light px-4">İptal</a>
                        <button type="submit" class="btn btn-primary px-5"><i class="bi bi-check2-circle me-1"></i> Şantiyeyi Oluştur</button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(new_view)
