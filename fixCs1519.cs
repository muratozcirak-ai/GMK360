using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Remove the errant characters
        string errant = @"            catch (Exception ex)
            {
                return Json(new { success = false, message = ""Blokları kaydederken hata oluştu: "" + ex.Message + (ex.InnerException != null ? "" - "" + ex.InnerException.Message : """") });
            }
        });
        }
public async Task<IActionResult> SaveStep1";

        string fixedCode = @"            catch (Exception ex)
            {
                return Json(new { success = false, message = ""Blokları kaydederken hata oluştu: "" + ex.Message + (ex.InnerException != null ? "" - "" + ex.InnerException.Message : """") });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveStep1";

        if (code.Contains(errant))
        {
            code = code.Replace(errant, fixedCode);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed syntax error 1.");
        }
        else
        {
            // fallback
            code = code.Replace("});\r\n        }\r\npublic async Task<IActionResult> SaveStep1", "}\r\n\r\n        [HttpPost]\r\n        public async Task<IActionResult> SaveStep1");
            code = code.Replace("});\n        }\npublic async Task<IActionResult> SaveStep1", "}\n\n        [HttpPost]\n        public async Task<IActionResult> SaveStep1");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Fixed syntax error fallback.");
        }
    }
}
