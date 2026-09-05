using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\murat\source\repos\GMK360\GMK360.Core\Entities\Construction\ConstructionProject.cs";
        string code = File.ReadAllText(path, Encoding.UTF8);

        if (!code.Contains("using GMK360.Core.Entities.Enums;"))
        {
            code = code.Replace("using System.Collections.Generic;", "using System.Collections.Generic;\r\nusing GMK360.Core.Entities.Enums;");
        }

        if (!code.Contains("public ProjectLifecycleStatus LifecycleStatus"))
        {
            code = code.Replace("public virtual ICollection<ConstructionTask> Tasks { get; set; }", "public ProjectLifecycleStatus LifecycleStatus { get; set; } = ProjectLifecycleStatus.UnderConstruction;\r\n        public virtual ICollection<ConstructionTask> Tasks { get; set; }");
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added LifecycleStatus to ConstructionProject.");
        }
    }
}
