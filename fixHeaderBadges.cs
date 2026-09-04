using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\ManageBlock.cshtml";
        string text = File.ReadAllText(path, Encoding.UTF8);

        string oldBtnStart = @"<button class=""accordion-button bg-transparent fw-bold collapsed flex-grow-1 shadow-none border-0"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#@collapseId"" aria-expanded=""false"" aria-controls=""@collapseId"">
                                        <i class=""bi bi-building-up me-2 text-primary""></i> @floorName
                                        <span class=""badge bg-secondary ms-3"">@(floorGroup.Count()) Birim</span>
                                    </button>";
                                    
        string newBtnStart = @"<button class=""accordion-button bg-transparent fw-bold collapsed flex-grow-1 shadow-none border-0"" type=""button"" data-bs-toggle=""collapse"" data-bs-target=""#@collapseId"" aria-expanded=""false"" aria-controls=""@collapseId"">
                                        <i class=""bi bi-building-up me-2 text-primary""></i> <span class=""text-dark me-2"">@floorName</span> <small class=""text-muted fw-normal"">(@(floorGroup.Count()) Birim)</small>
                                    </button>";

        if (text.Contains(oldBtnStart))
        {
            text = text.Replace(oldBtnStart, newBtnStart);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Accordion header badges removed");
        }
        else
        {
            Console.WriteLine("Target not found");
        }
        
        // Let's also check if there are other badges in the header from older versions
        string fallbackOld = @"<span class=""badge bg-secondary ms-auto me-3"">@(floorGroup.Count()) Birim</span>";
        if (text.Contains(fallbackOld))
        {
            text = text.Replace(fallbackOld, @"<small class=""text-muted ms-2 fw-normal"">(@(floorGroup.Count()) Birim)</small>");
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("Fallback removed");
        }
    }
}
