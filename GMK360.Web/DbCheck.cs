using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DbCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            using (var ctx = new ApplicationDbContext(optionsBuilder.Options))
            {
                var istanbul = ctx.Cities.FirstOrDefault(c => c.Name.Contains("ISTANBUL") || c.Name.Contains("İSTANBUL"));
                if (istanbul != null)
                {
                    var districts = ctx.Districts.Count(d => d.CityId == istanbul.Id);
                    Console.WriteLine($"Istanbul ID: {istanbul.Id}, Districts count: {districts}");
                }
                else
                {
                    Console.WriteLine("Istanbul not found");
                }
            }
        }
    }
}
