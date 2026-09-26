import re

filepath = r'GMK360.Web\Views\Definition\Values.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

edit_modal_old = r"""<div class="modal fade text-start" id="editModal-@val.Id" tabindex="-1" aria-labelledby="editModalLabel-@val.Id" aria-hidden="true">
                                        <div class="modal-dialog">
                                            <div class="modal-content">
                                                <div class="modal-header">
                                                    <h5 class="modal-title" id="editModalLabel-@val.Id">Seçeneği Düzenle</h5>
                                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                                </div>
                                                <form asp-action="EditValue" method="post">
                                                    <div class="modal-body">
                                                        <input type="hidden" name="id" value="@val.Id" />
                                                        <input type="hidden" name="categoryId" value="@category.Id" />
                                                        <div class="mb-3">
                                                            <label class="form-label">Seçenek Adı</label>
                                                            <input type="text" name="name" class="form-control" value="@val.Name" required />
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label">Sistem Kodu</label>
                                                            <input type="text" name="systemCode" class="form-control" value="@val.SystemCode" />
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label">Alt Seçenekler (Virgülle)</label>
                                                            <input type="text" name="subOptions" class="form-control" value="@val.SubOptions" />
                                                        </div>
                                                        <div class="mb-3 form-check form-switch">
                                                            <input class="form-check-input" type="checkbox" name="hasCount" value="true" @(val.HasCount ? "checked" : "")>
                                                            <label class="form-check-label">Adet Sorulsun mu?</label>
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label">Sıra No</label>
                                                            <input type="number" name="order" class="form-control" value="@val.Order" required />
                                                        </div>
                                                    </div>
                                                    <div class="modal-footer">
                                                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">İptal</button>
                                                        <button type="submit" class="btn btn-primary">Kaydet</button>
                                                    </div>
                                                </form>
                                            </div>
                                        </div>
                                    </div>"""

edit_modal_new = """<div class="modal fade text-start" id="editModal-@val.Id" tabindex="-1" aria-labelledby="editModalLabel-@val.Id" aria-hidden="true">
                                        <div class="modal-dialog">
                                            <div class="modal-content">
                                                <div class="modal-header">
                                                    <h5 class="modal-title fw-bold" id="editModalLabel-@val.Id">Seçeneği Düzenle</h5>
                                                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                                </div>
                                                <form asp-action="EditValue" method="post">
                                                    <div class="modal-body">
                                                        <input type="hidden" name="id" value="@val.Id" />
                                                        <input type="hidden" name="categoryId" value="@category.Id" />
                                                        <input type="hidden" name="order" value="@val.Order" />
                                                        
                                                        <div class="mb-3">
                                                            <label class="form-label fw-bold small">Seçenek Adı</label>
                                                            <input type="text" name="name" class="form-control" value="@val.Name" required />
                                                        </div>
                                                        
                                                        @if (category.SystemCode != "B2BSectors" && category.SystemCode != "B2BUstaSectors") {
                                                        <div class="mb-3">
                                                            <label class="form-label fw-bold small">Sistem Kodu</label>
                                                            <input type="text" name="systemCode" class="form-control" value="@val.SystemCode" />
                                                        </div>
                                                        <div class="mb-3">
                                                            <label class="form-label fw-bold small">Alt Seçenekler (Virgülle ayırın)</label>
                                                            <input type="text" name="subOptions" class="form-control" value="@val.SubOptions" />
                                                        </div>
                                                        <div class="mb-3 form-check form-switch bg-light p-2 rounded">
                                                            <input class="form-check-input ms-0 me-2" type="checkbox" name="hasCount" value="true" @(val.HasCount ? "checked" : "")>
                                                            <label class="form-check-label small fw-bold">Adet Sorulsun mu?</label>
                                                        </div>
                                                        }
                                                    </div>
                                                    <div class="modal-footer border-0 pt-0">
                                                        <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                                                        <button type="submit" class="btn btn-primary rounded-pill px-4 fw-bold">Kaydet</button>
                                                    </div>
                                                </form>
                                            </div>
                                        </div>
                                    </div>"""

content = re.sub(r'<div class="modal fade text-start".*?</form>\s*</div>\s*</div>\s*</div>', edit_modal_new, content, flags=re.DOTALL | re.IGNORECASE)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
