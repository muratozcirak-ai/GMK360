using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Web\Controllers\ConstructionProjectController.cs";
        var text = File.ReadAllText(path, Encoding.UTF8);

        string oldDoc = @"                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = ""/uploads/architecture/"" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User),
                    UploadDate = DateTime.UtcNow
                };";
                
        string newDoc = @"                var defaultFolder = await _context.DmsFolders.FirstOrDefaultAsync();
                if (defaultFolder == null) 
                {
                    defaultFolder = new GMK360.Core.Entities.DmsFolder { Name = ""Sistem Klasörü"", IsSystemFolder = true, CreatedDate = DateTime.UtcNow, AgencyId = block.ConstructionProject.AgencyId };
                    _context.DmsFolders.Add(defaultFolder);
                    await _context.SaveChangesAsync();
                }

                var doc = new GMK360.Core.Entities.DmsDocument
                {
                    Title = title ?? file.FileName,
                    DocumentUrl = ""/uploads/architecture/"" + uniqueFileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSizeBytes = file.Length,
                    EntityType = entityType, 
                    EntityId = entityId,
                    UploadedByUserId = _userManager.GetUserId(User) ?? """",
                    UploadDate = DateTime.UtcNow,
                    FolderId = defaultFolder.Id,
                    PhysicalLocationNote = ""Dijital Kopya""
                };";

        if (text.Contains(oldDoc))
        {
            text = text.Replace(oldDoc, newDoc);
            File.WriteAllText(path, text, new UTF8Encoding(true));
            Console.WriteLine("UploadArchitectureMedia Fixed");
        }
        else
        {
            Console.WriteLine("oldDoc not found");
        }
    }
}
