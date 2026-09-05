using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Context;
using GMK360.Core.Entities.Construction;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main()
    {
        var builder = new ConfigurationBuilder().AddJsonFile(@"c:\Users\murat\source\repos\GMK360\GMK360.Web\appsettings.json");
        var config = builder.Build();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(config.GetConnectionString("DefaultConnection")).Options;
        
        using (var db = new ApplicationDbContext(options))
        {
            var projects = db.ConstructionProjects.ToList();
            foreach(var p in projects) {
                Console.WriteLine($"ID: {p.Id}, Name: {p.Name}, Address: {p.Address}, City: {p.CityId}, Dist: {p.DistrictId}, St: {p.StreetId}");
            }
        }
    }
}
