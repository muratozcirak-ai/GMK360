import re

with open(r"GMK360.Web\Views\DocumentArchive\Index.cshtml", "r", encoding="utf-8") as f:
    content = f.read()

modal_html = """
<!-- Yeni Belge Yükle Modal -->
<div class="modal fade" id="uploadDocModal" tabindex="-1">
    <div class="modal-dialog">
        <form asp-action="UploadManual" asp-controller="DocumentArchive" method="post" enctype="multipart/form-data">
            <div class="modal-content border-0 shadow">
                <div class="modal-header bg-primary text-white border-0">
                    <h5 class="modal-title fw-bold"><i class="bi bi-cloud-upload me-2"></i> Belge Yükle</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body p-4">
                    <div class="mb-3">
                        <label class="form-label fw-bold">Dosya Seçin</label>
                        <input type="file" name="file" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Kategori (Raf)</label>
                        <select name="category" class="form-select" required>
                            @foreach (var cat in categories)
                            {
                                <option value="@cat">@cat</option>
                            }
                        </select>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">İlgili Proje (Dolap)</label>
                        <select name="projectId" class="form-select">
                            <option value="">-- Projeden Bağımsız --</option>
                            @if(projects != null)
                            {
                                foreach (var p in projects)
                                {
                                    if (selectedProjectId == p.Id)
                                    {
                                        <option value="@p.Id" selected>@p.Name</option>
                                    }
                                    else
                                    {
                                        <option value="@p.Id">@p.Name</option>
                                    }
                                }
                            }
                        </select>
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">İlgili Yıl</label>
                        <input type="number" name="year" class="form-control" value="@DateTime.Now.Year" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label fw-bold">Açıklama / Not (Opsiyonel)</label>
                        <textarea name="notes" class="form-control" rows="2"></textarea>
                    </div>
                </div>
                <div class="modal-footer border-0 bg-light">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-primary px-4"><i class="bi bi-upload me-2"></i> Yükle ve Kaydet</button>
                </div>
            </div>
        </form>
    </div>
</div>
"""

if "id=\"uploadDocModal\"" not in content:
    content = content + "\n" + modal_html
    with open(r"GMK360.Web\Views\DocumentArchive\Index.cshtml", "w", encoding="utf-8") as f:
        f.write(content)
    print("Added uploadDocModal.")
else:
    print("uploadDocModal already exists.")

