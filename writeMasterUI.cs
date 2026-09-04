using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        
        string content = @"@model GMK360.Core.Entities.Building

@{
    ViewData[""Title""] = ""Blok Yönetimi: "" + Model.BlockName;
    Layout = ""~/Views/Shared/_ConstructionLayout.cshtml"";
    
    var unitsByFloor = Model.Units?
        .GroupBy(u => u.FloorLevel)
        .OrderByDescending(g => g.Key)
        .ToList();
        
    var blockImages = ViewBag.BlockImages as List<GMK360.Core.Entities.DmsDocument> ?? new List<GMK360.Core.Entities.DmsDocument>();
    var floorPlans = ViewBag.FloorPlans as List<GMK360.Core.Entities.DmsDocument> ?? new List<GMK360.Core.Entities.DmsDocument>();
}

<div class=""d-flex justify-content-between align-items-center mb-4"">
    <div>
        <h2 class=""fw-bold mb-1""><i class=""bi bi-layers text-primary me-2""></i>@Model.BlockName Yönetimi</h2>
        <p class=""text-muted mb-0""><i class=""bi bi-building me-1""></i>Proje: @Model.ConstructionProject?.Name</p>
    </div>
    <a asp-action=""Details"" asp-route-id=""@Model.ConstructionProjectId"" class=""btn btn-outline-secondary rounded-pill px-4"">
        <i class=""bi bi-arrow-left me-2""></i> Proje Detayına Dön
    </a>
</div>

@if (TempData[""SuccessMessage""] != null)
{
    <div class=""alert alert-success alert-dismissible fade show rounded-4 border-0 shadow-sm mb-4"" role=""alert"">
        <i class=""bi bi-check-circle-fill me-2""></i> @TempData[""SuccessMessage""]
        <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
    </div>
}

<div class=""row g-4"">
    <div class=""col-md-4"">
        <!-- Blok Özellikleri -->
        <div class=""card border-0 shadow-sm rounded-4 mb-4"">
            <div class=""card-body p-4"">
                <div class=""d-flex justify-content-between align-items-center border-bottom pb-2 mb-3"">
                    <h5 class=""fw-bold mb-0""><i class=""bi bi-info-square text-info me-2""></i>Blok Özellikleri</h5>
                    <button class=""btn btn-sm btn-icon btn-light-primary rounded-circle"" data-bs-toggle=""modal"" data-bs-target=""#editBlockDetailsModal"" title=""Düzenle"">
                        <i class=""bi bi-pencil""></i>
                    </button>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Taban Oturumu</span>
                    <span class=""fw-bold text-dark"">@(Model.BaseArea.HasValue ? Model.BaseArea.Value + "" m²"" : ""Belirtilmedi"")</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Otopark</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.ParkingType) ? ""Belirtilmedi"" : Model.ParkingType)</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Asansör</span>
                    <span class=""fw-bold text-dark"">@(Model.ElevatorCount.HasValue ? Model.ElevatorCount + "" Adet"" : ""Belirtilmedi"")</span>
                </div>
                <div class=""mb-3"">
                    <span class=""text-muted small d-block"">Mantolama / Yalıtım</span>
                    <span class=""fw-bold text-dark"">@(string.IsNullOrEmpty(Model.InsulationType) ? ""Belirtilmedi"" : Model.InsulationType)</span>
                </div>
                <div>
                    <span class=""text-muted small d-block"">Açıklama</span>
                    <span class=""text-dark small"">@(string.IsNullOrEmpty(Model.Description) ? ""Belirtilmedi"" : Model.Description)</span>
                </div>
            </div>
        </div>

        <!-- Mimari Dış Cephe Çizimler Paneli -->
        <div class=""card border-0 shadow-sm rounded-4 mb-4"">
            <div class=""card-header bg-white border-bottom p-4 d-flex justify-content-between align-items-center"">
                <h6 class=""fw-bold mb-0""><i class=""bi bi-images text-primary me-2""></i>Blok Dış Cephe Renderları</h6>
            </div>
            <div class=""card-body p-4"">
                <div class=""row g-2 mb-3"">
                    @foreach(var img in blockImages)
                    {
                        <div class=""col-6 position-relative"">
                            <a href=""@img.DocumentUrl"" target=""_blank"">
                                <img src=""@img.DocumentUrl"" class=""img-fluid rounded border shadow-sm"" style=""height: 100px; object-fit: cover; width:100%;"" />
                            </a>
                            <form asp-action=""DeleteArchitectureMedia"" method=""post"" class=""position-absolute top-0 end-0 m-1"">
                                <input type=""hidden"" name=""documentId"" value=""@img.Id"" />
                                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                <button type=""submit"" class=""btn btn-danger btn-sm p-1 rounded-circle"" onclick=""return confirm('Silmek istediğinize emin misiniz?');""><i class=""bi bi-x""></i></button>
                            </form>
                            <div class=""small text-center mt-1 text-truncate"">@img.Title</div>
                        </div>
                    }
                </div>
                
                @if (blockImages.Count < 4)
                {
                    <form asp-action=""UploadArchitectureMedia"" method=""post"" enctype=""multipart/form-data"" class=""bg-light p-3 rounded border"">
                        <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                        <input type=""hidden"" name=""entityType"" value=""Building"" />
                        <input type=""hidden"" name=""entityId"" value=""@Model.Id"" />
                        <input type=""hidden"" name=""title"" value=""Dış Cephe Görseli"" />
                        
                        <label class=""small fw-bold mb-1"">Yeni Dış Cephe Görseli Yükle</label>
                        <div class=""input-group input-group-sm"">
                            <input type=""file"" name=""file"" class=""form-control"" accept=""image/*"" required />
                            <button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload""></i></button>
                        </div>
                    </form>
                }
            </div>
        </div>
    </div>
    
    <div class=""col-md-8"">
        <!-- Daire Listesi (Ağaç Görünümü) -->
        <div class=""card border-0 shadow-sm rounded-4 h-100"">
            <div class=""card-body p-4"">
                <div class=""d-flex justify-content-between align-items-center border-bottom pb-2 mb-4"">
                    <h5 class=""fw-bold mb-0""><i class=""bi bi-diagram-3 text-navy me-2""></i>Bağımsız Bölümler ve Toplu Düzenleme</h5>
                    <div>
                        <button type=""button"" class=""btn btn-sm btn-primary rounded-pill px-3 me-2"" data-bs-toggle=""modal"" data-bs-target=""#bulkEditUnitsModal"">
                            <i class=""bi bi-pencil-square me-1""></i> Seçili Daireleri Toplu Düzenle
                        </button>
                        <span class=""badge bg-light text-dark border rounded-pill px-3 py-2"">Toplam @(Model.Units?.Count ?? 0) Adet</span>
                    </div>
                </div>
                
                @if (unitsByFloor != null && unitsByFloor.Any())
                {
                    <div class=""accordion"" id=""floorAccordion"">
                        @foreach (var floorGroup in unitsByFloor)
                        {
                            var floorName = floorGroup.First().FloorName ?? $""{floorGroup.Key}. Kat"";
                            var headingId = $""heading{floorGroup.Key}"".Replace(""-"", ""minus"");
                            var collapseId = $""collapse{floorGroup.Key}"".Replace(""-"", ""minus"");
                            
                            // Kat planını bul
                            var planDoc = floorPlans.FirstOrDefault(d => d.EntityId == floorGroup.Key);
                            
                            <div class=""accordion-item border-0 mb-3 shadow-sm rounded-4 overflow-hidden"">
                                <h2 class=""accordion-header"" id=""@headingId"">
                                    <button class=""accordion-button bg-light fw-bold collapsed"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#@collapseId"" aria-expanded=""false"" aria-controls=""@collapseId"">
                                        <i class=""bi bi-building-up me-2 text-primary""></i> @floorName
                                        <span class=""badge bg-secondary ms-auto me-3"">@(floorGroup.Count()) Birim</span>
                                        @if(planDoc != null)
                                        {
                                            <span class=""badge bg-success me-2""><i class=""bi bi-file-earmark-image""></i> Plan Yüklü</span>
                                        }
                                    </button>
                                </h2>
                                <!-- COLLAPSED by default (removed 'show' class) -->
                                <div id=""@collapseId"" class=""accordion-collapse collapse"" aria-labelledby=""@headingId"">
                                    <div class=""accordion-body p-0"">
                                        <table class=""table table-hover align-middle mb-0"">
                                            <thead class=""table-light small"">
                                                <tr>
                                                    <th class=""ps-4"" style=""width:40px;"">
                                                        <div class=""form-check"">
                                                            <input class=""form-check-input select-all-floor"" type=""checkbox"" data-floor=""@floorGroup.Key"">
                                                        </div>
                                                    </th>
                                                    <th>No</th>
                                                    <th>Plan</th>
                                                    <th>Cephe / Alan</th>
                                                    <th>Mal Sahibi</th>
                                                    <th class=""text-end pe-4"">İşlemler</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                @foreach (var unit in floorGroup.OrderBy(u => u.Id))
                                                {
                                                    <tr>
                                                        <td class=""ps-4"">
                                                            <div class=""form-check"">
                                                                <input class=""form-check-input unit-checkbox"" type=""checkbox"" name=""selectedUnitIds"" value=""@unit.Id"" data-floor=""@floorGroup.Key"">
                                                            </div>
                                                        </td>
                                                        <td class=""fw-bold text-navy"">No: @unit.DoorNumber</td>
                                                        <td><span class=""badge bg-light text-dark border""><i class=""bi bi-aspect-ratio me-1""></i>@unit.RoomLayout</span></td>
                                                        <td class=""small"">
                                                            <div class=""text-muted"">Cephe: @(string.IsNullOrEmpty(unit.FacadeDirection) ? ""-"" : unit.FacadeDirection)</div>
                                                            <div class=""text-muted"">Brüt: @(unit.GrossSquareMeters?.ToString() ?? ""-"") m²</div>
                                                        </td>
                                                        <td>
                                                            @if (!string.IsNullOrEmpty(unit.OwnerName))
                                                            {
                                                                <span class=""text-dark small""><i class=""bi bi-person-circle me-1""></i>@unit.OwnerName</span>
                                                            }
                                                            else
                                                            {
                                                                <span class=""text-muted small fst-italic"">Sahipsiz / Boş</span>
                                                            }
                                                        </td>
                                                        <td class=""text-end pe-4"">
                                                            <a asp-controller=""CustomerPortal"" asp-action=""MyUnitMaterials"" asp-route-unitId=""@unit.Id"" class=""btn btn-sm btn-outline-primary rounded-pill me-1"" title=""Müşteri Seçim Portalı (Görünüm)""><i class=""bi bi-shop""></i> Portal</a>
                                                            <button class=""btn btn-sm btn-outline-secondary rounded-pill"" title=""Düzenle""><i class=""bi bi-pencil""></i></button>
                                                        </td>
                                                    </tr>
                                                }
                                            </tbody>
                                        </table>
                                        
                                        <!-- KAT PLANI BÖLÜMÜ -->
                                        <div class=""bg-white p-3 text-end border-top d-flex justify-content-between align-items-center"">
                                            @if(planDoc != null)
                                            {
                                                <div>
                                                    <a href=""@planDoc.DocumentUrl"" target=""_blank"" class=""btn btn-sm btn-success rounded-pill""><i class=""bi bi-file-earmark-image me-1""></i> @floorName Planını Görüntüle</a>
                                                </div>
                                                <form asp-action=""DeleteArchitectureMedia"" method=""post"" class=""d-inline"">
                                                    <input type=""hidden"" name=""documentId"" value=""@planDoc.Id"" />
                                                    <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                    <button type=""submit"" class=""btn btn-sm btn-outline-danger rounded-pill"" onclick=""return confirm('Kat planını silmek istediğinize emin misiniz?');"">Planı Sil</button>
                                                </form>
                                            }
                                            else
                                            {
                                                <div class=""w-100"">
                                                    <form asp-action=""UploadArchitectureMedia"" method=""post"" enctype=""multipart/form-data"" class=""d-flex align-items-center justify-content-end"">
                                                        <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                                                        <input type=""hidden"" name=""entityType"" value=""BuildingFloor_@Model.Id"" />
                                                        <input type=""hidden"" name=""entityId"" value=""@floorGroup.Key"" />
                                                        <input type=""hidden"" name=""title"" value=""@floorName Planı"" />
                                                        
                                                        <label class=""small text-muted me-2"">Bu Kata Ait Planı Yükle:</label>
                                                        <div class=""input-group input-group-sm"" style=""max-width: 300px;"">
                                                            <input type=""file"" name=""file"" class=""form-control"" accept=""image/*,.pdf"" required />
                                                            <button type=""submit"" class=""btn btn-primary""><i class=""bi bi-upload""></i></button>
                                                        </div>
                                                    </form>
                                                </div>
                                            }
                                        </div>
                                    </div>
                                </div>
                            </div>
                        }
                    </div>
                }
                else
                {
                    <div class=""text-center py-5 text-muted"">
                        <i class=""bi bi-tree"" style=""font-size: 3rem;""></i>
                        <p class=""mt-3"">Bu blokta henüz hiçbir daire veya dükkan oluşturulmamış.</p>
                        <p class=""small"">Daireleri ekleyebileceğiniz detaylı yönetim arayüzü yakında aktif edilecektir.</p>
                    </div>
                }
            </div>
        </div>
    </div>
</div>

<!-- Edit Block Details Modal -->
<div class=""modal fade"" id=""editBlockDetailsModal"" tabindex=""-1"">
    <div class=""modal-dialog modal-lg"">
        <div class=""modal-content rounded-4 border-0 shadow"">
            <form asp-action=""UpdateBlockDetails"" method=""post"">
                <input type=""hidden"" name=""Id"" value=""@Model.Id"" />
                <div class=""modal-header border-0 pb-0"">
                    <h5 class=""modal-title fw-bold""><i class=""bi bi-pencil-square text-primary me-2""></i>Blok Özelliklerini Düzenle</h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
                </div>
                <div class=""modal-body"">
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Blok Adı</label>
                            <input type=""text"" name=""BlockName"" class=""form-control"" value=""@Model.BlockName"" required />
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Taban Oturumu (m²)</label>
                            <input type=""number"" step=""0.1"" name=""BaseArea"" class=""form-control"" value=""@Model.BaseArea"" />
                        </div>
                    </div>

                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Asansör Sayısı</label>
                            <input type=""number"" name=""ElevatorCount"" class=""form-control"" value=""@Model.ElevatorCount"" placeholder=""Örn: 2"" />
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Otopark Tipi</label>
                            <select name=""ParkingType"" class=""form-select"">
                                <option value="""" selected=""@(string.IsNullOrEmpty(Model.ParkingType))"">Seçiniz...</option>
                                <option value=""Kapalı Otopark"" selected=""@(Model.ParkingType == ""Kapalı Otopark"")"">Kapalı Otopark</option>
                                <option value=""Açık Otopark"" selected=""@(Model.ParkingType == ""Açık Otopark"")"">Açık Otopark</option>
                                <option value=""Açık ve Kapalı Otopark"" selected=""@(Model.ParkingType == ""Açık ve Kapalı Otopark"")"">Açık ve Kapalı Otopark</option>
                                <option value=""Yok"" selected=""@(Model.ParkingType == ""Yok"")"">Yok</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Mantolama / Yalıtım</label>
                        <input type=""text"" name=""InsulationType"" class=""form-control"" placeholder=""Örn: Taş Yünü"" value=""@Model.InsulationType"" />
                    </div>
                    
                    <div class=""mb-4 p-3 bg-light rounded"">
                        <label class=""form-label fw-bold text-primary mb-3""><i class=""bi bi-ui-checks-grid me-1""></i> Diğer Donanım ve Altyapılar</label>
                        <div class=""row g-2"">
                            @{
                                var existingFeatures = Model.TechnicalFeatures ?? """";
                                string[] predefined = { ""Su Deposu"", ""Sığınak"", ""Jeneratör"", ""Fiber İnternet"", ""Uydu Sistemi"", ""Görüntülü Diyafon"", ""Şifreli Giriş"", ""Kamera Sistemi"", ""Akıllı Ev Altyapısı"", ""Engelli Rampası"" };
                            }
                            @foreach(var item in predefined)
                            {
                                <div class=""col-md-4 col-sm-6"">
                                    <div class=""form-check form-check-custom"">
                                        <input class=""form-check-input"" type=""checkbox"" name=""SelectedFeatures"" value=""@item"" id=""feat_@item.Replace("" "", """")"" @(existingFeatures.Contains(item) ? ""checked"" : """") />
                                        <label class=""form-check-label text-dark small"" for=""feat_@item.Replace("" "", """")"">@item</label>
                                    </div>
                                </div>
                            }
                        </div>
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Özel Eklemeler ve Notlar</label>
                        <div class=""form-text mb-2"">Yukarıdaki listede olmayan ekstra bir özellik varsa (örn: Güneş Paneli) veya detay yazmak istiyorsanız buraya ekleyebilirsiniz.</div>
                        <textarea name=""TechnicalFeatures"" class=""form-control"" rows=""2""></textarea>
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Genel Açıklama</label>
                        <textarea name=""Description"" class=""form-control"" rows=""2"">@Model.Description</textarea>
                    </div>
                </div>
                <div class=""modal-footer border-0 pt-0"">
                    <button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>
                    <button type=""submit"" class=""btn btn-primary rounded-pill px-4"">Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<!-- Bulk Edit Units Modal (Daire Özellikleri Toplu Düzenleme) -->
<div class=""modal fade"" id=""bulkEditUnitsModal"" tabindex=""-1"">
    <div class=""modal-dialog"">
        <div class=""modal-content rounded-4 border-0 shadow"">
            <form id=""bulkEditForm"" method=""post"">
                <!-- We will populate this hidden input via JavaScript with selected unit IDs -->
                <input type=""hidden"" name=""unitIds"" id=""bulkUnitIds"" value="""" />
                
                <div class=""modal-header border-0 pb-0"">
                    <h5 class=""modal-title fw-bold""><i class=""bi bi-collection text-primary me-2""></i>Toplu Daire Düzenleme</h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
                </div>
                <div class=""modal-body"">
                    <div class=""alert alert-info py-2 small"">
                        <i class=""bi bi-info-circle me-1""></i> Sadece doldurduğunuz alanlar seçili dairelere uygulanacaktır. Boş bıraktığınız alanlar değişmeden kalır.
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Cephe Değiştir</label>
                        <select name=""FacadeDirection"" class=""form-select"">
                            <option value="""">Değiştirme (Aynı Kalsın)</option>
                            <option value=""Kuzey"">Kuzey</option>
                            <option value=""Güney"">Güney</option>
                            <option value=""Doğu"">Doğu</option>
                            <option value=""Batı"">Batı</option>
                            <option value=""Kuzey-Doğu"">Kuzey-Doğu</option>
                            <option value=""Kuzey-Batı"">Kuzey-Batı</option>
                            <option value=""Güney-Doğu"">Güney-Doğu</option>
                            <option value=""Güney-Batı"">Güney-Batı</option>
                        </select>
                    </div>
                    
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Brüt Alan (m²)</label>
                            <input type=""number"" step=""0.1"" name=""GrossSquareMeters"" class=""form-control"" placeholder=""Değiştirme"" />
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Net Alan (m²)</label>
                            <input type=""number"" step=""0.1"" name=""NetSquareMeters"" class=""form-control"" placeholder=""Değiştirme"" />
                        </div>
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold small"">Daire Planı (Oda Sayısı vb.)</label>
                        <select name=""RoomLayout"" class=""form-select"">
                            <option value="""">Değiştirme</option>
                            <option value=""Stüdyo"">Stüdyo</option>
                            <option value=""1+1"">1+1</option>
                            <option value=""2+1"">2+1</option>
                            <option value=""3+1"">3+1</option>
                            <option value=""4+1"">4+1</option>
                            <option value=""Dükkan"">Dükkan / Ticari</option>
                            <option value=""Depo"">Depo</option>
                        </select>
                    </div>
                </div>
                <div class=""modal-footer border-0 pt-0"">
                    <button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>
                    <button type=""button"" class=""btn btn-primary rounded-pill px-4"" id=""btnSubmitBulkEdit"">Seçili Dairelere Uygula</button>
                </div>
            </form>
        </div>
    </div>
</div>

<style>
    .accordion-button:not(.collapsed) {
        color: #0d6efd;
        background-color: #f8f9fa;
        box-shadow: none;
    }
    .accordion-button:focus {
        box-shadow: none;
        border-color: rgba(0,0,0,.125);
    }
</style>

@section Scripts {
    <script>
        $(document).ready(function() {
            // Floor checkbox toggles all units in that floor
            $('.select-all-floor').change(function() {
                var floor = $(this).data('floor');
                var isChecked = $(this).is(':checked');
                $('.unit-checkbox[data-floor=""' + floor + '""]').prop('checked', isChecked);
            });
            
            // Bulk Edit Submit
            $('#btnSubmitBulkEdit').click(function() {
                var selectedIds = [];
                $('.unit-checkbox:checked').each(function() {
                    selectedIds.push($(this).val());
                });
                
                if (selectedIds.length === 0) {
                    alert('Lütfen düzenlemek için en az bir daire seçin.');
                    return;
                }
                
                $('#bulkUnitIds').val(selectedIds.join(','));
                
                // Submit via AJAX or regular form
                alert('Toplu düzenleme işlemi (ID: ' + selectedIds.join(',') + ') henüz C# tarafına bağlanmadı, bir sonraki adımda bağlanacak!');
                $('#bulkEditUnitsModal').modal('hide');
            });
        });
    </script>
}
";
        File.WriteAllText(path, content, new UTF8Encoding(true));
        Console.WriteLine("Master UI Written");
    }
}
