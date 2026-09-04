$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$oldHeader = @"
                  <div class="d-flex justify-content-between align-items-center border-bottom pb-2 mb-4">
                      <h5 class="fw-bold mb-0"><i class="bi bi-diagram-3 text-navy me-2"></i>Mimari Ağaç ve Bağımsız Bölümler</h5>
                      <span class="badge bg-primary rounded-pill px-3 py-2 ms-3 flex-shrink-0">Toplam @(Model.Units?.Count ?? 0) Adet</span>
                  </div>
"@

$newHeader = @"
                  <div class="d-flex align-items-center gap-3 border-bottom pb-2 mb-4">
                      <h5 class="fw-bold mb-0 m-0"><i class="bi bi-diagram-3 text-navy me-2"></i>Mimari Ağaç ve Bağımsız Bölümler</h5>
                      <span class="badge bg-primary rounded-pill px-3 py-2 ms-auto flex-shrink-0">Toplam @(Model.Units?.Count ?? 0) Adet</span>
                  </div>
"@

$text = $text.Replace($oldHeader, $newHeader)
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
