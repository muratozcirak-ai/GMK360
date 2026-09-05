using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Infrastructure.Data;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;MultipleActiveResultSets=true");
        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            var projs = context.ConstructionProjects.ToList();
            Console.WriteLine($"Total Projects: {projs.Count}");
            foreach (var p in projs)
            {
                Console.WriteLine($"ID: {p.Id}, Name: {p.Name}, Status: {p.Status}, Apts: {p.TargetTotalApartments}, Shops: {p.TargetTotalShops}, Created: {p.CreatedAt}");
            }
        }
    }
}
