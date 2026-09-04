using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Amenities.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        // 1. Add Total Land Area card at the top
        string headerSearch = @"<div class=""row g-4"">";
        string topCard = @"
<div class=""card shadow-sm rounded-4 border-0 mb-4"">
    <div class=""card-body"">
        <form asp-action=""UpdateProjectLandArea"" method=""post"" class=""d-flex flex-wrap align-items-end gap-4"">
            <input type=""hidden"" name=""projectId"" value=""@Model.Id"" />
            <div class=""flex-grow-1"">
                <label class=""form-label fw-bold"">Toplam Arazi Alanı (m²)</label>
                <input type=""number"" step=""0.1"" name=""totalLandArea"" class=""form-control form-control-solid"" value=""@Model.TotalLandArea"" placeholder=""Örn: 2500"" />
            </div>
            <div class=""flex-grow-1"">
                <label class=""form-label fw-bold"">Peyzaj / Yeşil Alan (m²)</label>
                <input type=""number"" step=""0.1"" name=""landscapeArea"" class=""form-control form-control-solid"" value=""@Model.LandscapeArea"" placeholder=""Örn: 800"" />
            </div>
            <div>
                <button type=""submit"" class=""btn btn-primary rounded-pill px-4""><i class=""bi bi-check2""></i> Araziyi Kaydet</button>
            </div>
        </form>
    </div>
</div>
";
        text = text.Replace(headerSearch, topCard + headerSearch);


        // 2. Hide "Kategori Tipi" in the ADD form
        string oldCategory = @"<div class=""mb-4"">
                        <label class=""form-label fw-bold fs-7"">Kategori Tipi</label>
                        <select name=""type"" class=""form-select form-select-solid rounded-3"" required>
                            <option value=""Peyzaj"">Peyzaj / Yeşil Alan</option>
                            <option value=""Otopark"">Otopark</option>
                            <option value=""Sosyal Tesis"">Sosyal Tesis (Çardak vb.)</option>
                            <option value=""Spor Alanı"">Spor Alanı / Havuz</option>
                            <option value=""Diğer"">Diğer Dış Alan</option>
                        </select>
                    </div>";
        string newCategory = @"<input type=""hidden"" name=""type"" value=""Dış Alan"" />";
        text = text.Replace(oldCategory, newCategory);

        // Remove Kategori from the table
        text = text.Replace(@"<th class=""min-w-140px"">Kategori</th>", "");
        text = text.Replace(@"<td>
                                            <span class=""badge bg-secondary text-dark"">@item.Type</span>
                                        </td>", "");


        // 3. Add Edit functionality (Modal)
        // First, add the Edit button next to Delete
        string oldActions = @"<form asp-action=""DeleteAmenity"" method=""post"" onsubmit=""return confirm('Bu açık alanı silmek istediğinize emin misiniz?');"">";
        string newActions = @"<button type=""button"" class=""btn btn-icon btn-light-primary btn-sm me-2"" onclick=""openEditModal(@item.Id, '@item.Name', '@item.SquareMeters', '@item.Description')"">
                                                    <i class=""bi bi-pencil""></i>
                                                </button>
                                                " + oldActions;
        text = text.Replace(oldActions, newActions);

        // Add the Modal at the bottom
        string editModal = @"
<!-- Edit Modal -->
<div class=""modal fade"" id=""editAmenityModal"" tabindex=""-1"">
    <div class=""modal-dialog modal-dialog-centered"">
        <div class=""modal-content border-0 shadow rounded-4"">
            <div class=""modal-header"">
                <h5 class=""modal-title fw-bold"">Alanı Düzenle</h5>
                <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
            </div>
            <div class=""modal-body"">
                <form asp-action=""EditAmenity"" method=""post"">
                    <input type=""hidden"" name=""projectId"" value=""@Model.Id"" />
                    <input type=""hidden"" name=""amenityId"" id=""editAmenityId"" />
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold"">Alan Adı</label>
                        <input type=""text"" name=""name"" id=""editAmenityName"" class=""form-control"" required />
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold"">Metrekare (m²)</label>
                        <input type=""number"" step=""0.1"" name=""squareMeters"" id=""editAmenitySq"" class=""form-control"" />
                    </div>
                    
                    <div class=""mb-3"">
                        <label class=""form-label fw-bold"">Kısa Açıklama</label>
                        <textarea name=""description"" id=""editAmenityDesc"" class=""form-control"" rows=""2""></textarea>
                    </div>
                    
                    <button type=""submit"" class=""btn btn-primary w-100 rounded-pill"">Kaydet</button>
                </form>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        function openEditModal(id, name, sq, desc) {
            document.getElementById('editAmenityId').value = id;
            document.getElementById('editAmenityName').value = name;
            document.getElementById('editAmenitySq').value = sq;
            document.getElementById('editAmenityDesc').value = desc;
            var modal = new bootstrap.Modal(document.getElementById('editAmenityModal'));
            modal.show();
        }
    </script>
}
";
        text = text + editModal;

        File.WriteAllText(path, text, new UTF8Encoding(true));
    }
}
