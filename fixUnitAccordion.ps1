$files = @("c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageUnit.cshtml", "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\TemplateSpaces.cshtml")

foreach($path in $files)
{
    $text = [System.IO.File]::ReadAllText($path)

    $pattern = '(?s)<div class="d-flex w-100 align-items-center pe-5">.*?<div class="text-truncate me-auto fw-bold">(.*?)</div>.*?<div class="flex-shrink-0 text-end">.*?<span class="badge bg-white text-dark border me-2">@space\.Type</span>.*?@if\(space\.SquareMeters\.HasValue\).*?{.*?<span class="badge[^>]*">@space\.SquareMeters m²</span>.*?}.*?</div>.*?</div>'

    $replacement = '<span class="w-100 text-truncate fw-bold">$1 <small class="text-muted fw-normal ms-2">(@space.Type @if(space.SquareMeters.HasValue){<text>- @space.SquareMeters m²</text>})</small></span>'

    $text = [System.Text.RegularExpressions.Regex]::Replace($text, $pattern, $replacement)

    # I also need to handle the old ManageUnit code if it didn't match the new regex
    $oldPattern = '(?s)<span class="text-truncate me-auto">(.*?)</span>.*?<div class="ms-3 me-3 flex-shrink-0">.*?<span class="badge bg-white text-dark border me-2">@space\.Type</span>.*?@if\(space\.SquareMeters\.HasValue\).*?{.*?<span class="badge[^>]*">@space\.SquareMeters m²</span>.*?}.*?</div>'
    $oldReplacement = '<span class="w-100 text-truncate fw-bold">$1 <small class="text-muted fw-normal ms-2">(@space.Type @if(space.SquareMeters.HasValue){<text>- @space.SquareMeters m²</text>})</small></span>'
    
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, $oldPattern, $oldReplacement)

    [System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
}
