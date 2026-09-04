using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldJs = @"                // Submit via AJAX or regular form
                alert('Toplu düzenleme işlemi (ID: ' + selectedIds.join(',') + ') henüz C# tarafına bağlanmadı, bir sonraki adımda bağlanacak!');
                $('#bulkEditUnitsModal').modal('hide');";
                
        string newJs = @"                // Set action and submit form
                $('#bulkEditForm').attr('action', '/ConstructionProject/BulkUpdateUnits?buildingId=@Model.Id');
                $('#bulkEditForm').submit();";

        if (text.Contains(oldJs))
        {
            text = text.Replace(oldJs, newJs);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("JS Fixed");
        }
        else
        {
            Console.WriteLine("JS block not found");
        }
    }
}
