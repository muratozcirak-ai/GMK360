$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\Shared\_ConstructionLayout.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$text = $text.Replace('<div class="container mt-4">', '<div class="container-fluid px-4 px-xl-5 mt-4">')
$text = $text.Replace('<div class="col-md-3">', '<div class="col-xl-2 col-lg-3 col-md-4">')
$text = $text.Replace('<div class="col-md-9">', '<div class="col-xl-10 col-lg-9 col-md-8">')

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
