using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Models\CreateProjectWizardViewModel.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        if (!code.Contains("DraftProjectId"))
        {
            code = code.Replace("public string Name { get; set; }", "public int DraftProjectId { get; set; }\n\n        [Required(ErrorMessage = \"Proje adı zorunludur.\")]\n        public string Name { get; set; }");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added DraftProjectId.");
        }
    }
}
