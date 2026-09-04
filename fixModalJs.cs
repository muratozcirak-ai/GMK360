using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // 1. Give IDs to the row and alert so we can hide them
        string oldAlert = @"<div class=""alert alert-info py-2 small mb-4"">
                          <i class=""bi bi-info-circle me-1""></i> Yeni bir kata birim eklerseniz o kat otomatik olarak oluşturulur.
                      </div>";
        string newAlert = @"<div class=""alert alert-info py-2 small mb-4"" id=""addUnitAlert"">
                          <i class=""bi bi-info-circle me-1""></i> Yeni bir kata birim eklerseniz o kat otomatik olarak oluşturulur.
                      </div>";
        view = Regex.Replace(view, @"<div class=""alert alert-info py-2 small mb-4"">.*?</div>", newAlert, RegexOptions.Singleline);

        string oldRow = @"<div class=""row"">
                          <div class=""col-md-6 mb-3"">
                              <label class=""form-label fw-bold small"">Kat Seviyesi \(Sayısal\)</label>";
        string newRow = @"<div class=""row"" id=""floorDataRow"">
                          <div class=""col-md-6 mb-3"">
                              <label class=""form-label fw-bold small"">Kat Seviyesi (Sayısal)</label>";
        view = Regex.Replace(view, @"<div class=""row"">\s*<div class=""col-md-6 mb-3"">\s*<label class=""form-label fw-bold small"">Kat Seviyesi", newRow);

        // 2. Update JS
        string oldJs = @"\$('.btn-add-unit-to-floor').click\(function\(\) \{.*?rowIdx = 1;\s*\}\);";
        string newJs = @"$('.btn-add-unit-to-floor').click(function() {
                  var floorLevel = $(this).data('floorlevel');
                  var floorName = $(this).data('floorname');
                  $('input[name=""FloorLevel""]').val(floorLevel);
                  $('input[name=""FloorName""]').val(floorName);
                  $('#addUnitModalTitle').text(floorName + ' - Hızlı Birim Ekle');
                  $('#floorDataRow').hide();
                  $('#addUnitAlert').hide();
              });

              $('[data-bs-target=""#addUnitModal""]:not(.btn-add-unit-to-floor)').click(function() {
                  $('input[name=""FloorLevel""]').val('');
                  $('input[name=""FloorName""]').val('');
                  $('#addUnitModalTitle').text('Yeni Kat / Birim Ekle');
                  $('#floorDataRow').show();
                  $('#addUnitAlert').show();
              });";
        view = Regex.Replace(view, oldJs, newJs, RegexOptions.Singleline);

        File.WriteAllText(viewPath, view, new UTF8Encoding(true));
        Console.WriteLine("Modal JS updated.");
    }
}
