$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$text = $text.Replace('<div class="col-md-3">', '<div class="col-xl-3 col-lg-4 mb-4">')
$text = $text.Replace('<div class="col-md-9">', '<div class="col-xl-9 col-lg-8">')

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
