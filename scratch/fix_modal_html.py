import re

filepath = r'GMK360.Web\Views\Admin\B2bList.cshtml'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the modal HTML
start_idx = content.find('<div class="modal fade" id="addModal" tabindex="-1">')
end_idx = content.find('@section Scripts')

modal_html = """<div class="modal fade" id="addModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content" style="border-radius: 15px;">
            <div class="modal-header border-0 pb-0">
                <h5 class="modal-title fw-bold">Yeni @(listType == "usta" ? "Usta" : (listType == "tedarikci" ? "Tedarikçi" : "Firma")) Ekle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form asp-action="AddB2bCompany" method="post">
                <input type="hidden" name="listType" value="@listType" />
                <div class="modal-body">
                    
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Ünvan / İsim</label>
                        <input type="text" class="form-control" placeholder="Örn: Ahmet Yılmaz veya XYZ İnşaat A.Ş." name="name" required />
                    </div>

                    <!-- Hukuki Statü -->
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Hukuki Statü</label>
                        <div class="d-flex gap-4">
                            <div class="form-check">
                                <input class="form-check-input legal-status-radio" type="radio" name="legalStatus" id="legalIndividual" value="1" checked>
                                <label class="form-check-label" for="legalIndividual">Şahıs (Bireysel)</label>
                            </div>
                            <div class="form-check">
                                <input class="form-check-input legal-status-radio" type="radio" name="legalStatus" id="legalCorporate" value="2">
                                <label class="form-check-label" for="legalCorporate">Şirket (Tüzel Kişi)</label>
                            </div>
                        </div>
                    </div>

                    <!-- Şahıs Alanları -->
                    <div id="individualFields">
                        <div class="mb-3">
                            <label class="form-label fw-bold small">TC Kimlik No</label>
                            <input type="text" class="form-control" name="tcKimlik" placeholder="İsteğe Bağlı" />
                        </div>
                    </div>

                    <!-- Şirket Alanları -->
                    <div id="corporateFields" style="display:none;">
                        <div class="row">
                            <div class="col-6 mb-3">
                                <label class="form-label fw-bold small">Vergi Dairesi</label>
                                <input type="text" class="form-control" name="taxOffice" />
                            </div>
                            <div class="col-6 mb-3">
                                <label class="form-label fw-bold small">Vergi No</label>
                                <input type="text" class="form-control" name="taxNumber" />
                            </div>
                        </div>
                        <div class="mb-3 form-check form-switch bg-light p-3 rounded">
                            <input class="form-check-input ms-0 me-2" type="checkbox" name="isEnterprise" id="isEnterprise" value="true">
                            <label class="form-check-label small fw-bold" for="isEnterprise">Bu firmanın şubeleri / alt bayi ağı var (Kurumsal Ağ)</label>
                        </div>
                    </div>

                    <!-- Ortak İletişim / Lokasyon -->
                    <div class="mb-3">
                        <label class="form-label fw-bold small">Telefon Numarası</label>
                        <input type="text" class="form-control" placeholder="05XX XXX XX XX" name="phone" required />
                    </div>

                    <div class="row">
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold small">Hizmet İli</label>
                            <select class="form-select select2-search" id="modalCity" name="cityId" style="width: 100%;" required>
                                <option value="">İl Seçin</option>
                                @if(cities != null)
                                {
                                    foreach(var c in cities)
                                    {
                                        <option value="@c.Id">@c.Name</option>
                                    }
                                }
                            </select>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold small">İlçe</label>
                            <select class="form-select select2-search" id="modalDistrict" name="districtId" style="width: 100%;">
                                <option value="">İlçe Seçin</option>
                            </select>
                        </div>
                        <div class="col-4 mb-3">
                            <label class="form-label fw-bold small">Mahalle</label>
                            <select class="form-select select2-search" id="modalNeighborhood" name="neighborhoodId" style="width: 100%;">
                                <option value="">Mahalle Seçin</option>
                            </select>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label fw-bold small">@(listType == "usta" ? "Ustalık Alanları" : "Sektörleri")</label>
                        <select class="form-select select2-search" multiple="multiple" name="categoryIds" style="width: 100%;" required>
                            @if(categories != null)
                            {
                                foreach(var cat in categories)
                                {
                                    <option value="@cat.Id">@cat.Name</option>
                                }
                            }
                        </select>
                    </div>
                </div>
                <div class="modal-footer border-0 pt-0">
                    <button type="button" class="btn btn-secondary rounded-pill px-4" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn @btnColor rounded-pill px-4 fw-bold">Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

"""

new_content = content[:start_idx] + modal_html + content[end_idx:]

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(new_content)
