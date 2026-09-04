$viewPath1 = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text1 = [System.IO.File]::ReadAllText($viewPath1)
$text1 = $text1.Replace('<div class="d-flex justify-content-between align-items-center w-100 me-3">', '')
$text1 = $text1.Replace('<span><i class="bi bi-building-up me-2 text-primary"></i> @floorName</span>', '<span class="text-truncate me-auto"><i class="bi bi-building-up me-2 text-primary"></i> @floorName</span>')
$text1 = $text1.Replace('<div>', '<div class="ms-3 me-3 flex-shrink-0">')
$text1 = $text1.Replace('</div>
                                        </div>
                                    </button>', '</div>
                                    </button>')
[System.IO.File]::WriteAllText($viewPath1, $text1, [System.Text.Encoding]::UTF8)

$viewPath2 = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageUnit.cshtml"
$text2 = [System.IO.File]::ReadAllText($viewPath2)

$text2 = $text2.Replace('<div class="d-flex justify-content-between align-items-center w-100 me-3">', '')
$text2 = $text2.Replace('<span><i class="bi bi-bounding-box-circles me-2 text-secondary"></i> @space.Name</span>', '<span class="text-truncate me-auto"><i class="bi bi-bounding-box-circles me-2 text-secondary"></i> @space.Name</span>')
$text2 = $text2.Replace('<div>', '<div class="ms-3 me-3 flex-shrink-0">')
$text2 = $text2.Replace('</div>
                                            </div>
                                        </button>', '</div>
                                        </button>')

# Fix Card Header overlap
$text2 = $text2.Replace('<div class="card-header bg-white border-0 pt-4 pb-0 d-flex justify-content-between align-items-center">', '<div class="card-header bg-white border-0 pt-4 pb-0 d-flex justify-content-between align-items-start">')

[System.IO.File]::WriteAllText($viewPath2, $text2, [System.Text.Encoding]::UTF8)
