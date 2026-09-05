using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Views\ConstructionProject\Create.cshtml";
        string code = File.ReadAllText(path, Encoding.UTF8);

        // Remove the black button
        string blackButton = @"<div class=""col-md-4 d-flex align-items-end"">\s*<button type=""button"" class=""btn btn-dark w-100 py-2 rounded-3"" onclick=""generateBlocks\(\)"">Blokları Oluştur<\/button>\s*<\/div>";
        if (Regex.IsMatch(code, blackButton)) {
            code = Regex.Replace(code, blackButton, "");
            Console.WriteLine("Removed black button.");
        } else {
            Console.WriteLine("Black button not found.");
        }
        
        // Remove 'readonly' condition from 'Tek Blok' input so user can edit it
        code = code.Replace(@"${count === 1 ? 'readonly' : ''}", "");

        // Make blockCount trigger generateBlocks on input
        string blockCountInput = @"<input type=""number"" id=""blockCount"" class=""form-control py-2 rounded-3"" min=""1"" value=""1"" />";
        string newBlockCountInput = @"<input type=""number"" id=""blockCount"" class=""form-control py-2 rounded-3"" min=""1"" value=""1"" oninput=""generateBlocks()"" />";
        if (code.Contains(blockCountInput)) {
            code = code.Replace(blockCountInput, newBlockCountInput);
            Console.WriteLine("Added oninput to blockCount.");
        } else {
            Console.WriteLine("blockCount input string not found.");
        }

        string oldGenerateBlocks = @"container\.innerHTML = '';\s*const letters = ""ABCDEFGHIJKLMNOPQRSTUVWXYZ"";\s*for\(let i = 0; i < count; i\+\+\) \{";
        string newGenerateBlocks = @"const letters = ""ABCDEFGHIJKLMNOPQRSTUVWXYZ"";
              
              const currentBlocks = container.querySelectorAll('.block-item');
              const currentCount = currentBlocks.length;
              
              if (count < currentCount) {
                  // Remove excess blocks from the end
                  for (let i = currentCount - 1; i >= count; i--) {
                      currentBlocks[i].remove();
                  }
              } else if (count > currentCount) {
                  // Add new blocks
                  for (let i = currentCount; i < count; i++) {";

        if (Regex.IsMatch(code, oldGenerateBlocks)) {
            code = Regex.Replace(code, oldGenerateBlocks, newGenerateBlocks);
            
            // Fix appending string logic inside the loop
            string oldAppend = @"container.innerHTML \+= html;";
            string newAppend = @"container.insertAdjacentHTML('beforeend', html);";
            code = Regex.Replace(code, oldAppend, newAppend);

            Console.WriteLine("Updated generateBlocks logic.");
        } else {
            Console.WriteLine("generateBlocks regex not found.");
        }

        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Done.");
    }
}
