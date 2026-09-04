using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldTbodyEnd = @"                                            </tbody>
                                        </table>";
                                        
        string newTbodyEnd = @"                                            </tbody>
                                            <tfoot class=""table-light"">
                                                <tr>
                                                    <td colspan=""6"" class=""text-center py-2"">
                                                        <button type=""button"" class=""btn btn-sm btn-link text-success text-decoration-none fw-bold btn-add-unit-to-floor"" data-bs-toggle=""modal"" data-bs-target=""#addUnitModal"" data-floorlevel=""@floorGroup.Key"" data-floorname=""@floorName"">
                                                            <i class=""bi bi-plus-circle me-1""></i> Bu Kata Yeni Birim Ekle
                                                        </button>
                                                    </td>
                                                </tr>
                                            </tfoot>
                                        </table>";

        string jsStart = @"$(document).ready(function() {";
        string newJs = @"$(document).ready(function() {
            
            // Populate Add Unit Modal when clicked from a specific floor
            $('.btn-add-unit-to-floor').click(function() {
                var floorLevel = $(this).data('floorlevel');
                var floorName = $(this).data('floorname');
                $('#addUnitModal input[name=""FloorLevel""]').val(floorLevel);
                $('#addUnitModal input[name=""FloorName""]').val(floorName);
            });
";

        bool changed = false;
        if (text.Contains(oldTbodyEnd))
        {
            text = text.Replace(oldTbodyEnd, newTbodyEnd);
            changed = true;
        }
        
        if (text.Contains(jsStart))
        {
            text = text.Replace(jsStart, newJs);
            changed = true;
        }

        if (changed)
        {
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Added Floor-Specific Unit Add Button");
        }
    }
}
