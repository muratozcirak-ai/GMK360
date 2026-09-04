using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GMK360.Data.Contexts;
using GMK360.Core.Entities;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=GMK360_DB;Trusted_Connection=True;MultipleActiveResultSets=true");

        using (var context = new ApplicationDbContext(optionsBuilder.Options))
        {
            var spaces = context.UnitSpaces.ToList();
            foreach (var space in spaces)
            {
                if (space.Type != null)
                {
                    space.Type = space.Type.Replace("YaÅŸam AlanÄ±", "Yaşam Alanı");
                    space.Type = space.Type.Replace("SirkÃ¼lasyon", "Sirkülasyon");
                    space.Type = space.Type.Replace("Islak Hacim", "Islak Hacim"); // just in case
                }
                if (space.Name != null)
                {
                    space.Name = space.Name.Replace("DÃ¼kkan", "Dükkan");
                }
            }
            
            var units = context.BuildingUnits.ToList();
            foreach(var u in units)
            {
                if(u.DoorNumber != null) u.DoorNumber = u.DoorNumber.Replace("DÃ¼kkan", "Dükkan");
                if(u.FloorName != null) u.FloorName = u.FloorName.Replace("SÄ±ÄŸÄ±nak", "Sığınak");
            }
            
            context.SaveChanges();
        }
    }
}
