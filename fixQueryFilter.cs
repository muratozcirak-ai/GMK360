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

        string oldOnModelCreating = @"protected override void OnModelCreating\(ModelBuilder builder\)\s*\{\s*base\.OnModelCreating\(builder\);";
        
        string newOnModelCreating = @"protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply Global Query Filter for Soft Delete
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(GMK360.Core.Entities.BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ApplicationDbContext).GetMethod(nameof(SetGlobalQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var genericMethod = method.MakeGenericMethod(entityType.ClrType);
                    genericMethod.Invoke(this, new object[] { builder });
                }
            }";

        if (!code.Contains("Apply Global Query Filter for Soft Delete"))
        {
            code = Regex.Replace(code, oldOnModelCreating, newOnModelCreating);
            File.WriteAllText(path, code, new UTF8Encoding(true));
            Console.WriteLine("Added Reflection to OnModelCreating.");
        }
    }
}
