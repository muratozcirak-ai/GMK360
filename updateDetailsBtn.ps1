$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Details.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$oldBtns = @"
        <a asp-controller="ProjectMaterial" asp-action="Index" asp-route-projectId="@Model.Id" class="btn btn-primary rounded-pill px-4 shadow-sm">
            <i class="bi bi-palette me-2"></i> Müşteri Malzeme Kataloğu
        </a>
"@

$newBtns = @"
        <a asp-action="Templates" asp-route-projectId="@Model.Id" class="btn btn-info rounded-pill px-4 shadow-sm text-white me-2">
            <i class="bi bi-collection me-2"></i> Daire Şablonları
        </a>
        <a asp-controller="ProjectMaterial" asp-action="Index" asp-route-projectId="@Model.Id" class="btn btn-primary rounded-pill px-4 shadow-sm">
            <i class="bi bi-palette me-2"></i> Müşteri Malzeme Kataloğu
        </a>
"@

$text = $text.Replace($oldBtns, $newBtns)
[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
