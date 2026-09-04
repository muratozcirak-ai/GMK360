$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$text = $text.Replace('<div class="col-md-3 mb-4">', '<div class="col-xl-2 col-lg-3 col-md-4 mb-4">')

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
