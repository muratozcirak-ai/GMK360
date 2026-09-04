using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string viewPath = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string view = File.ReadAllText(viewPath, Encoding.UTF8);

        // Replace row and alert IDs safely
        view = view.Replace("<div class=\"row\">\r\n                          <div class=\"col-md-6 mb-3\">\r\n                              <label class=\"form-label fw-bold small\">Kat Seviyesi", 
                            "<div class=\"row\" id=\"floorDataRow\">\r\n                          <div class=\"col-md-6 mb-3\">\r\n                              <label class=\"form-label fw-bold small\">Kat Seviyesi");
        
        view = view.Replace("<div class=\"row\">\n                          <div class=\"col-md-6 mb-3\">\n                              <label class=\"form-label fw-bold small\">Kat Seviyesi", 
                            "<div class=\"row\" id=\"floorDataRow\">\n                          <div class=\"col-md-6 mb-3\">\n                              <label class=\"form-label fw-bold small\">Kat Seviyesi");

        // JS replacement: Find the start and end of the block
        int start = view.IndexOf("$('.btn-add-unit-to-floor').click(function() {");
        int end = view.IndexOf("rowIdx = 1;\r\n            });", start);
        if (end == -1) end = view.IndexOf("rowIdx = 1;\n            });", start);
        
        if (start != -1 && end != -1)
        {
            end += "rowIdx = 1;\r\n            });".Length;
            string oldJsBlock = view.Substring(start, end - start);
            
            string newJsBlock = @"$('.btn-add-unit-to-floor').click(function() {
                  var floorLevel = $(this).data('floorlevel');
                  var floorName = $(this).data('floorname');
                  $('[name=""FloorLevel""]').val(floorLevel);
                  $('[name=""FloorName""]').val(floorName);
                  $('#addUnitModal .modal-title').html('<i class=""bi bi-plus-square text-success me-2""></i>' + floorName + ' - Hızlı Birim Ekle');
                  $('#floorDataRow').hide();
                  $('#addUnitAlert').hide();
              });

              $('[data-bs-target=""#addUnitModal""]:not(.btn-add-unit-to-floor)').click(function() {
                  $('[name=""FloorLevel""]').val('');
                  $('[name=""FloorName""]').val('');
                  $('#addUnitModal .modal-title').html('<i class=""bi bi-plus-square text-success me-2""></i>Yeni Kat / Birim Ekle');
                  $('#floorDataRow').show();
                  $('#addUnitAlert').show();
              });";
            
            view = view.Replace(oldJsBlock, newJsBlock);
            File.WriteAllText(viewPath, view, new UTF8Encoding(true));
            Console.WriteLine("Replaced JS block perfectly.");
        }
        else
        {
            Console.WriteLine("Could not find JS block limits.");
        }
    }
}
