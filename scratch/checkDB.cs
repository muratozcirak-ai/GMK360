using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Context;
using GMK360.Core.Entities;

class Program {
    static void Main() {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GMK360Db;Trusted_Connection=True;");
        using var context = new ApplicationDbContext(optionsBuilder.Options);
        
        var blocks = context.Buildings.ToList();
        foreach (var b in blocks) {
            Console.WriteLine($"Block: {b.Name}, Existing: {b.IsExistingBuilding}, TotalUnits: {b.TotalUnits}, TotalShops: {b.TotalShops}, TotalApartments: {b.TotalApartments}");
        }
    }
}
