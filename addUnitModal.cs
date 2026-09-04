using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldBtnGroup = @"                        <button type=""button"" class=""btn btn-sm btn-primary rounded-pill px-3 me-2"" data-bs-toggle=""modal"" data-bs-target=""#bulkEditUnitsModal"">
                            <i class=""bi bi-pencil-square me-1""></i> Seçili Daireleri Toplu Düzenle
                        </button>";
                        
        string newBtnGroup = @"                        <button type=""button"" class=""btn btn-sm btn-success rounded-pill px-3 me-2"" data-bs-toggle=""modal"" data-bs-target=""#addUnitModal"">
                            <i class=""bi bi-plus-circle me-1""></i> Yeni Kat / Birim Ekle
                        </button>
                        <button type=""button"" class=""btn btn-sm btn-primary rounded-pill px-3 me-2"" data-bs-toggle=""modal"" data-bs-target=""#bulkEditUnitsModal"">
                            <i class=""bi bi-pencil-square me-1""></i> Seçili Daireleri Toplu Düzenle
                        </button>";

        string newModal = @"
<!-- Add Unit Modal -->
<div class=""modal fade"" id=""addUnitModal"" tabindex=""-1"">
    <div class=""modal-dialog"">
        <div class=""modal-content rounded-4 border-0 shadow"">
            <form id=""addUnitForm"" action=""#"" method=""post"">
                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                
                <div class=""modal-header border-0 pb-0"">
                    <h5 class=""modal-title fw-bold""><i class=""bi bi-plus-square text-success me-2""></i>Yeni Bağımsız Bölüm Ekle</h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
                </div>
                <div class=""modal-body"">
                    <div class=""alert alert-info py-2 small mb-4"">
                        <i class=""bi bi-info-circle me-1""></i> Yeni bir kata birim eklerseniz o kat otomatik olarak oluşturulur.
                    </div>
                    
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Kat Seviyesi (Sayısal)</label>
                            <input type=""number"" name=""FloorLevel"" class=""form-control"" placeholder=""Örn: 1, 0, -1"" required />
                            <div class=""form-text"" style=""font-size:11px;"">Zemin: 0, Bodrum: -1, Normal: 1</div>
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Kat Adı (Görünüm)</label>
                            <input type=""text"" name=""FloorName"" class=""form-control"" placeholder=""Örn: Zemin Kat"" />
                        </div>
                    </div>
                    
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Kapı No</label>
                            <input type=""text"" name=""DoorNumber"" class=""form-control"" required />
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Birim Tipi / Plan</label>
                            <select name=""RoomLayout"" class=""form-select"">
                                <option value=""3+1"" selected>3+1 Daire</option>
                                <option value=""2+1"">2+1 Daire</option>
                                <option value=""1+1"">1+1 Daire</option>
                                <option value=""4+1"">4+1 Daire</option>
                                <option value=""Dükkan"">Dükkan / Ticari</option>
                                <option value=""Depo"">Depo</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class=""row"">
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Brüt Alan (m²)</label>
                            <input type=""number"" step=""0.1"" name=""GrossSquareMeters"" class=""form-control"" />
                        </div>
                        <div class=""col-md-6 mb-3"">
                            <label class=""form-label fw-bold small"">Cephe</label>
                            <select name=""FacadeDirection"" class=""form-select"">
                                <option value="""">Seçiniz...</option>
                                <option value=""Kuzey"">Kuzey</option>
                                <option value=""Güney"">Güney</option>
                                <option value=""Doğu"">Doğu</option>
                                <option value=""Batı"">Batı</option>
                                <option value=""Kuzey-Güney"">Kuzey-Güney (Çift)</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class=""modal-footer border-0 pt-0"">
                    <button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>
                    <button type=""button"" class=""btn btn-success rounded-pill px-4"" onclick=""alert('C# Ekleme Action\'ı az sonra yazılacak!')"">Bölümü Ekle</button>
                </div>
            </form>
        </div>
    </div>
</div>
";

        if (text.Contains(oldBtnGroup))
        {
            text = text.Replace(oldBtnGroup, newBtnGroup);
            
            // Insert modal before style tag
            int styleIdx = text.IndexOf("<style>");
            if (styleIdx != -1)
            {
                text = text.Insert(styleIdx, newModal);
                File.WriteAllText(path, text, new UTF8Encoding(true));
                Console.WriteLine("Add Unit UI Added");
            }
        }
        else
        {
            Console.WriteLine("Buttons not found");
        }
    }
}
