using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Data\Contexts\ApplicationDbContext.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        string oldLog = @"AuditLogs.Add\(new AuditLog \{\s*ActionType = ""SOFT_DELETE"",\s*EntityName = entry\.Entity\.GetType\(\)\.Name,\s*EntityId = entity\.Id\.ToString\(\),\s*Timestamp = DateTime\.UtcNow\s*\}\);";
        
        string newLog = @"AuditLogs.Add(new AuditLog {
                        ActionType = ""SOFT_DELETE"",
                        EntityName = entry.Entity.GetType().Name,
                        EntityId = entity.Id.ToString(),
                        Timestamp = DateTime.UtcNow,
                        OldValues = ""{}"",
                        NewValues = ""{\""IsDeleted\"": true}"",
                        AffectedColumns = ""[\""IsDeleted\""]"",
                        UserId = ""SYSTEM""
                    });";

        code = Regex.Replace(code, oldLog, newLog);
        
        File.WriteAllText(path, code, new UTF8Encoding(true));
        Console.WriteLine("Fixed ApplicationDbContext AuditLog creation.");
    }
}
