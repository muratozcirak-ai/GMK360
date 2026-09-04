using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        string text = File.ReadAllText(path);

        string oldMB = @"var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == id);";

        string newMB = @"var block = await _context.Buildings
                .Include(b => b.ConstructionProject)
                .Include(b => b.Units)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (block != null)
            {
                ViewBag.UnitTemplates = await _context.UnitTemplates
                    .Where(t => t.ConstructionProjectId == block.ConstructionProjectId)
                    .ToListAsync();
            }";

        text = text.Replace(oldMB, newMB);
        File.WriteAllText(path, text, System.Text.Encoding.UTF8);
    }
}
