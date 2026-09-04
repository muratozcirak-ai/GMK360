$path = "c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml"
$text = [System.IO.File]::ReadAllText($path)

$text = $text.Replace('aria-controls="@collapseId""><span', 'aria-controls="@collapseId"><span')

[System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)
