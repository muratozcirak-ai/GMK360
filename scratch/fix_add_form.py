import re

filepath = r'GMK360.Web\Views\Definition\Values.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace ADD FORM
old_add_form = """<form asp-action="AddValue" method="post">
                <input type="hidden" name="categoryId" value="@category?.Id" />
                <div class="row g-3 align-items-center">
                    <div class="col-md-3">
                        <div class="form-floating">
                            <input type="text" name="name" id="valName" class="form-control" placeholder="Örn: Asansör" required />
                            <label for="valName">Seçenek Adı (Örn: Asansör)</label>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="form-floating">
                            <input type="text" name="systemCode" id="valSys" class="form-control" placeholder="Opsiyonel" />
                            <label for="valSys">Sistem Kodu</label>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="form-floating">
                            <input type="text" name="subOptions" id="valSub" class="form-control" placeholder="Örn: Kabin, Ebeveyn" />
                            <label for="valSub">Alt Seçenekler (Virgülle)</label>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="form-check form-switch mt-2">
                            <input class="form-check-input" type="checkbox" id="valHasCount" name="hasCount" value="true">
                            <label class="form-check-label fw-bold" for="valHasCount">Adet Sorulsun mu?</label>
                        </div>
                        <div class="form-floating mt-2">
                            <input type="number" name="order" id="valOrd" class="form-control form-control-sm" value="@(Model.Any() ? Model.Max(x => x.Order) + 1 : 1)" required />
                            <label for="valOrd">Sıra No</label>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <button type="submit" class="btn btn-primary w-100 py-3 fw-bold" style="border-radius: 10px;">
                            <i class="ph ph-plus me-1"></i> Ekle
                        </button>
                    </div>
                </div>
            </form>"""

new_add_form = """<form asp-action="AddValue" method="post">
                <input type="hidden" name="categoryId" value="@category?.Id" />
                <input type="hidden" name="order" value="@(Model.Any() ? Model.Max(x => x.Order) + 1 : 1)" />
                <div class="row g-3 align-items-center">
                    <div class="col-md-4">
                        <div class="form-floating">
                            <input type="text" name="name" id="valName" class="form-control" placeholder="Seçenek Adı" required />
                            <label for="valName">Seçenek Adı</label>
                        </div>
                    </div>
                    @if (category.SystemCode != "B2BSectors" && category.SystemCode != "B2BUstaSectors") {
                    <div class="col-md-2">
                        <div class="form-floating">
                            <input type="text" name="systemCode" id="valSys" class="form-control" placeholder="Opsiyonel" />
                            <label for="valSys">Sistem Kodu</label>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="form-floating">
                            <input type="text" name="subOptions" id="valSub" class="form-control" placeholder="Örn: Kabin, Ebeveyn" />
                            <label for="valSub">Alt Seçenekler (Virgülle)</label>
                        </div>
                    </div>
                    <div class="col-md-1">
                        <div class="form-check form-switch mt-2">
                            <input class="form-check-input" type="checkbox" id="valHasCount" name="hasCount" value="true">
                            <label class="form-check-label fw-bold" for="valHasCount">Adet?</label>
                        </div>
                    </div>
                    }
                    <div class="col-md-2">
                        <button type="submit" class="btn btn-primary w-100 py-3 fw-bold" style="border-radius: 10px;">
                            <i class="ph ph-plus me-1"></i> Ekle
                        </button>
                    </div>
                </div>
            </form>"""

# Need to replace regardless of encoding issues, I'll use regex or simpler replace.
content = re.sub(r'<form asp-action="AddValue" method="post">.*?</form>', new_add_form, content, flags=re.DOTALL | re.IGNORECASE)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
