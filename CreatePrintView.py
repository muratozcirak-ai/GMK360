import codecs
import os

dir_path = r'C:\Users\murat\source\repos\GMK360\GMK360.Web\Views\SubcontractorContract'
if not os.path.exists(dir_path):
    os.makedirs(dir_path)

filepath = os.path.join(dir_path, 'PrintPreview.cshtml')

content = '''@model GMK360.Core.Entities.Construction.SubcontractorContract
@{
    Layout = null; // Yazdırma sayfası olduğu için menüleri gizliyoruz
}
<!DOCTYPE html>
<html lang="tr">
<head>
    <meta charset="utf-8" />
    <title>@ViewBag.Title - Yazdır</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <style>
        body { background: #f0f2f5; font-family: 'Times New Roman', serif; }
        .a4-page {
            width: 21cm;
            min-height: 29.7cm;
            padding: 2cm;
            margin: 2cm auto;
            background: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            font-size: 12pt;
            line-height: 1.5;
        }
        @@media print {
            body { background: white; margin: 0; padding: 0; }
            .a4-page { width: 100%; margin: 0; padding: 1cm; box-shadow: none; }
            .no-print { display: none !important; }
        }
    </style>
</head>
<body>

    <!-- YAZDIRMA KONTROLLERİ (Ekranda görünür, kağıtta gizlenir) -->
    <div class="no-print bg-dark text-white p-3 shadow fixed-top d-flex justify-content-between align-items-center">
        <div>
            <strong>Sözleşme Önizleme:</strong> @ViewBag.Title
        </div>
        <div>
            <button onclick="window.print()" class="btn btn-primary"><i class="bi bi-printer"></i> Yazdır (Print)</button>
            <a href="/SubcontractorContract/Details/@Model.Id" class="btn btn-outline-light ms-2">Geri Dön</a>
        </div>
    </div>

    <!-- A4 SAYFASI -->
    <div class="a4-page mt-5">
        @Html.Raw(ViewBag.PrintContent)
    </div>

</body>
</html>
'''

with codecs.open(filepath, 'w', 'utf-8-sig') as f:
    f.write(content)
