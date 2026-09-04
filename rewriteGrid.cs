using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string htmlPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string html = File.ReadAllText(htmlPath, Encoding.UTF8);

        // Replace the entire Add Unit Modal
        string modalStart = @"<!-- Add Unit Modal -->";
        string modalEnd = @"<!-- Copy Floor Modal -->";
        
        string newModal = @"<!-- Add Unit Modal -->
<div class=""modal fade"" id=""addUnitModal"" tabindex=""-1"">
    <div class=""modal-dialog modal-lg"">
        <div class=""modal-content rounded-4 border-0 shadow"">
            <form id=""addUnitForm"" asp-action=""AddMultipleBuildingUnits"" method=""post"">
                <input type=""hidden"" name=""buildingId"" value=""@Model.Id"" />
                <input type=""hidden"" name=""FloorLevel"" id=""addUnitFloorLevel"" />
                <input type=""hidden"" name=""FloorName"" id=""addUnitFloorName"" />
                
                <div class=""modal-header border-0 pb-0"">
                    <h5 class=""modal-title fw-bold""><i class=""bi bi-plus-square text-success me-2""></i><span id=""addUnitModalTitle"">Birim Ekle</span></h5>
                    <button type=""button"" class=""btn-close"" data-bs-dismiss=""modal""></button>
                </div>
                <div class=""modal-body"">
                    <div class=""alert alert-info py-2 small mb-3"">
                        <i class=""bi bi-info-circle me-1""></i> Bu kata aynı anda birden fazla bağımsız bölüm ekleyebilirsiniz. 
                    </div>
                    
                    <div class=""table-responsive mb-2"">
                        <table class=""table table-sm table-bordered align-middle"" id=""dynamicUnitsTable"">
                            <thead class=""table-light"">
                                <tr>
                                    <th>Kapı No / İsim</th>
                                    <th>Yapı / Tür</th>
                                    <th>Cephe</th>
                                    <th>Brüt Alan (m²)</th>
                                    <th style=""width:40px;""></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td><input type=""text"" name=""Units[0].DoorNumber"" class=""form-control form-control-sm"" placeholder=""Örn: 15 veya Kuzey"" required /></td>
                                    <td>
                                        <select name=""Units[0].UnitStructure"" class=""form-select form-select-sm"">
                                            <option value=""Ara Kat"" selected>Ara Kat</option>
                                            <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                            <option value=""Ters Dubleks"">Ters Dubleks</option>
                                            <option value=""Bahçe Katı"">Bahçe Katı</option>
                                            <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                            <option value=""Su Deposu"">Su Deposu</option>
                                            <option value=""Sığınak"">Sığınak</option>
                                            <option value=""Otopark"">Otopark</option>
                                            <option value=""Depo"">Depo</option>
                                            <option value=""Ortak Alan"">Diğer Ortak Alan</option>
                                        </select>
                                    </td>
                                    <td>
                                        <select name=""Units[0].FacadeDirection"" class=""form-select form-select-sm"">
                                            <option value="""">Seçiniz...</option>
                                            <option value=""Kuzey"">Kuzey</option>
                                            <option value=""Güney"">Güney</option>
                                            <option value=""Doğu"">Doğu</option>
                                            <option value=""Batı"">Batı</option>
                                            <option value=""Kuzeydoğu"">Kuzeydoğu</option>
                                            <option value=""Kuzeybatı"">Kuzeybatı</option>
                                            <option value=""Güneydoğu"">Güneydoğu</option>
                                            <option value=""Güneybatı"">Güneybatı</option>
                                        </select>
                                    </td>
                                    <td><input type=""number"" name=""Units[0].GrossSquareMeters"" class=""form-control form-control-sm"" /></td>
                                    <td></td>
                                </tr>
                            </tbody>
                        </table>
                        <button type=""button"" class=""btn btn-sm btn-outline-success"" id=""btnAddNewUnitRow""><i class=""bi bi-plus-circle""></i> Yeni Satır Ekle</button>
                    </div>
                    <div class=""form-text text-info"" style=""font-size:11px;""><i class=""bi bi-info-circle me-1""></i>Aradığınız tür listede yoksa, Sistem Ayarları > Tanımlamalar menüsünden yeni tür ekleyebilirsiniz. (Not: 3+1 gibi detaylar daire iç ölçülerinde belirlenecektir.)</div>
                </div>
                <div class=""modal-footer border-0 pt-0"">
                    <button type=""button"" class=""btn btn-light rounded-pill px-4"" data-bs-dismiss=""modal"">İptal</button>
                    <button type=""submit"" class=""btn btn-success rounded-pill px-4""><i class=""bi bi-check-circle""></i> Hepsini Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<!-- Copy Floor Modal -->";

        if (html.Contains(modalStart) && html.Contains(modalEnd))
        {
            int startIdx = html.IndexOf(modalStart);
            int endIdx = html.IndexOf(modalEnd);
            string oldBlock = html.Substring(startIdx, endIdx - startIdx);
            html = html.Replace(oldBlock, newModal);
        }

        // Also update JS for adding row and populating the modal
        string jsAnchor = @"// Populate Add Unit Modal when clicked from a specific floor";
        string jsEnd = @"// Floor checkbox toggles all units in that floor";
        
        string newJs = @"// Populate Add Unit Modal when clicked from a specific floor
            var rowIdx = 1;
            $('#btnAddNewUnitRow').click(function() {
                var newRow = `<tr>
                                <td><input type=""text"" name=""Units[${rowIdx}].DoorNumber"" class=""form-control form-control-sm"" required /></td>
                                <td>
                                    <select name=""Units[${rowIdx}].UnitStructure"" class=""form-select form-select-sm"">
                                        <option value=""Ara Kat"" selected>Ara Kat</option>
                                        <option value=""Çatı Dubleksi"">Çatı Dubleksi</option>
                                        <option value=""Ters Dubleks"">Ters Dubleks</option>
                                        <option value=""Bahçe Katı"">Bahçe Katı</option>
                                        <option value=""Dükkan / Ticari"">Dükkan / Ticari</option>
                                        <option value=""Su Deposu"">Su Deposu</option>
                                        <option value=""Sığınak"">Sığınak</option>
                                        <option value=""Otopark"">Otopark</option>
                                        <option value=""Depo"">Depo</option>
                                        <option value=""Ortak Alan"">Diğer Ortak Alan</option>
                                    </select>
                                </td>
                                <td>
                                    <select name=""Units[${rowIdx}].FacadeDirection"" class=""form-select form-select-sm"">
                                        <option value="""">Seçiniz...</option>
                                        <option value=""Kuzey"">Kuzey</option>
                                        <option value=""Güney"">Güney</option>
                                        <option value=""Doğu"">Doğu</option>
                                        <option value=""Batı"">Batı</option>
                                        <option value=""Kuzeydoğu"">Kuzeydoğu</option>
                                        <option value=""Kuzeybatı"">Kuzeybatı</option>
                                        <option value=""Güneydoğu"">Güneydoğu</option>
                                        <option value=""Güneybatı"">Güneybatı</option>
                                    </select>
                                </td>
                                <td><input type=""number"" name=""Units[${rowIdx}].GrossSquareMeters"" class=""form-control form-control-sm"" /></td>
                                <td><button type=""button"" class=""btn btn-sm btn-danger remove-unit-row""><i class=""bi bi-trash""></i></button></td>
                              </tr>`;
                $('#dynamicUnitsTable tbody').append(newRow);
                rowIdx++;
            });

            $(document).on('click', '.remove-unit-row', function() {
                $(this).closest('tr').remove();
            });

            $('.btn-add-unit-to-floor').click(function() {
                var floorLevel = $(this).data('floorlevel');
                var floorName = $(this).data('floorname');
                $('#addUnitFloorLevel').val(floorLevel);
                $('#addUnitFloorName').val(floorName);
                $('#addUnitModalTitle').text(floorName + ' - Hızlı Birim Ekle');
                
                // Reset table to 1 row when opened
                $('#dynamicUnitsTable tbody tr:not(:first)').remove();
                $('#dynamicUnitsTable tbody tr:first input').val('');
                $('#dynamicUnitsTable tbody tr:first select').prop('selectedIndex',0);
                rowIdx = 1;
            });
            
            // For the global Add Floor button
            $('.btn-add-global').click(function(){
                 $('#addUnitModalTitle').text('Yeni Kat / Birim Ekle');
                 // For a global add, we need them to type the floor manually, but we hid it!
                 // Let's just prompt them for floor level if it's empty
            });

            // Floor checkbox toggles all units in that floor";

        if (html.Contains(jsAnchor) && html.Contains(jsEnd))
        {
            int startIdx = html.IndexOf(jsAnchor);
            int endIdx = html.IndexOf(jsEnd);
            string oldJs = html.Substring(startIdx, endIdx - startIdx);
            html = html.Replace(oldJs, newJs);
        }
        
        // Wait! The global "Yeni Kat / Birim Ekle" button at the top!
        // We hid the FloorLevel inputs! If they click the global button, they can't set FloorLevel!
        // Let's modify the global button to just open a prompt or let's just make the FloorLevel inputs visible again at the top of the modal!
        string modalBody = @"<div class=""alert alert-info py-2 small mb-3"">";
        string inputsHTML = @"
                    <div class=""row mb-3"">
                        <div class=""col-md-6"">
                            <label class=""form-label fw-bold small"">Kat Seviyesi (Sayısal)</label>
                            <input type=""number"" name=""FloorLevel"" id=""addUnitFloorLevel"" class=""form-control form-control-sm"" required />
                        </div>
                        <div class=""col-md-6"">
                            <label class=""form-label fw-bold small"">Kat Adı</label>
                            <input type=""text"" name=""FloorName"" id=""addUnitFloorName"" class=""form-control form-control-sm"" />
                        </div>
                    </div>
                    <div class=""alert alert-info py-2 small mb-3"">";
        html = html.Replace(@"<input type=""hidden"" name=""FloorLevel"" id=""addUnitFloorLevel"" />", "");
        html = html.Replace(@"<input type=""hidden"" name=""FloorName"" id=""addUnitFloorName"" />", "");
        html = html.Replace(modalBody, inputsHTML);

        File.WriteAllText(htmlPath, html, new UTF8Encoding(true));
        Console.WriteLine("Done rewriting grid modal.");
    }
}
