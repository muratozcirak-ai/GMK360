$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$oldSelect = @"
                    <div class="mb-4">
                        <label class="form-label fw-bold small">Standart Daire Planı</label>
                        <select name="roomLayout" class="form-select">
                            <option value="1+1">1+1</option>
                            <option value="2+1">2+1</option>
                            <option value="3+1" selected>3+1</option>
                            <option value="4+1">4+1</option>
                        </select>
                    </div>
"@

$newSelect = @"
                    <div class="mb-4">
                        <label class="form-label fw-bold small">Daire Şablonu (Opsiyonel)</label>
                        <select name="templateId" class="form-select">
                            <option value="">-- Şablon Seçilmedi (Boş Daire) --</option>
                            @if(ViewBag.UnitTemplates != null)
                            {
                                foreach(var tmpl in (IEnumerable<GMK360.Core.Entities.UnitTemplate>)ViewBag.UnitTemplates)
                                {
                                    <option value="@tmpl.Id">@tmpl.Name (@tmpl.RoomLayout)</option>
                                }
                            }
                        </select>
                        <div class="form-text small">Şablon seçerseniz, iç odalar otomatik kopyalanır. <a href="/ConstructionProject/Templates?projectId=@Model.ConstructionProjectId" target="_blank">Şablonları Yönet</a></div>
                    </div>
"@

$text = $text.Replace($oldSelect, $newSelect)
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
