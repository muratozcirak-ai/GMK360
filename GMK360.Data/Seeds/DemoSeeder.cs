using System;
using System.Linq;
using System.Threading.Tasks;
using GMK360.Core.Entities;
using GMK360.Core.Entities.Construction;
using GMK360.Core.Entities.Identity;
using GMK360.Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GMK360.Data.Seeds
{
    public static class DemoSeeder
    {
        public static async Task SeedDemoDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new ApplicationRole { Name = "Customer" });
            if (!await roleManager.RoleExistsAsync("AgencyAdmin"))
                await roleManager.CreateAsync(new ApplicationRole { Name = "AgencyAdmin" });

            var adminUser = await userManager.FindByEmailAsync("admin@gmk360.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = "admin@gmk360.com", Email = "admin@gmk360.com", FirstName = "Sistem", LastName = "Y�neticisi", EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "123456");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var customerUser = await userManager.FindByEmailAsync("musteri@gmk360.com");
            if (customerUser == null)
            {
                customerUser = new ApplicationUser { UserName = "musteri@gmk360.com", Email = "musteri@gmk360.com", FirstName = "Ahmet", LastName = "M��teri", EmailConfirmed = true };
                await userManager.CreateAsync(customerUser, "123456");
                await userManager.AddToRoleAsync(customerUser, "Customer");
            }

            var agency = await context.Agencies.FirstOrDefaultAsync();
            if (agency == null)
            {
                agency = new Agency { CompanyName = "GMK Mimarl�k ve �n�aat" };
                context.Agencies.Add(agency);
                await context.SaveChangesAsync();
            }

            var project = await context.ConstructionProjects.FirstOrDefaultAsync(p => p.Name == "GMK Ya�am Evleri");
            if (project == null)
            {
                project = new ConstructionProject
                {
                    AgencyId = agency.Id,
                    Name = "GMK Ya�am Evleri",
                    StartDate = DateTime.UtcNow.AddMonths(-2),
                    Status = GMK360.Core.Entities.Construction.ProjectStatus.Aktif_Santiye,
                    Address = "Merkez Mah. �n�aat Sk.",
                    CoverImageUrl = "https://images.unsplash.com/photo-1541888086225-f64069811c7f?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80"
                };
                context.ConstructionProjects.Add(project);
                await context.SaveChangesAsync();
                
                var phase1 = new ProjectPhase { ConstructionProjectId = project.Id, Name = "Hafriyat ve Temel", Description = "Kaz� ve temel beton", PlannedStartDate = DateTime.UtcNow.AddMonths(-2), Status = 2, OrderIndex = 1 };
                var phase2 = new ProjectPhase { ConstructionProjectId = project.Id, Name = "Kaba �n�aat", Description = "Kolon ve tabliyeler", PlannedStartDate = DateTime.UtcNow.AddMonths(-1), Status = 1, OrderIndex = 2 };
                var phase3 = new ProjectPhase { ConstructionProjectId = project.Id, Name = "�nce ���ilik", Description = "Duvar, s�va, elektrik", PlannedStartDate = DateTime.UtcNow.AddMonths(1), Status = 0, OrderIndex = 3 };
                context.ProjectPhases.AddRange(phase1, phase2, phase3);
                await context.SaveChangesAsync();

                context.PhaseTasks.Add(new PhaseTask { ProjectPhaseId = phase1.Id, Name = "Temel Kaz�s�", Status = "Tamamland�", OrderIndex = 1 });
                context.PhaseTasks.Add(new PhaseTask { ProjectPhaseId = phase2.Id, Name = "1. Kat Kolon Betonlar�", Status = "Devam Ediyor", OrderIndex = 1 });
                await context.SaveChangesAsync();

                var building = new Building { ConstructionProjectId = project.Id, BlockName = "A Blok", TotalFloors = 5 };
                context.Buildings.Add(building);
                await context.SaveChangesAsync();

                var unit1 = new BuildingUnit { BuildingId = building.Id, UnitNumber = "1", FloorLevel = 1, OwnerUserId = customerUser.Id, RoomLayout = "3+1" };
                var unit2 = new BuildingUnit { BuildingId = building.Id, UnitNumber = "2", FloorLevel = 1, RoomLayout = "2+1" };
                context.BuildingUnits.AddRange(unit1, unit2);
                await context.SaveChangesAsync();

                var materials = new[]
                {
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Zemin", MaterialName = "Koyu Gri Mat Seramik (Vitra)", Description = "Kayd�rmaz, ��k g�r�n�ml� modern seramik.", ImageUrl = "https://images.unsplash.com/photo-1584622650111-993a426fbf0a?auto=format&fit=crop&w=400&q=80", PriceDifference = 0 },
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Zemin", MaterialName = "Bej Mermer Desen (�anakkale)", Description = "Klasik banyolar i�in ferah g�r�n�m.", ImageUrl = "https://images.unsplash.com/photo-1620626011761-996317b8d101?auto=format&fit=crop&w=400&q=80", PriceDifference = 0 },
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Zemin", MaterialName = "Ah�ap G�r�n�ml� Seramik (K�tahya)", Description = "Do�al ah�ap s�cakl���n� banyoya ta��r.", ImageUrl = "https://images.unsplash.com/photo-1597405436665-2748259db813?auto=format&fit=crop&w=400&q=80", PriceDifference = 2500 },
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Zemin", MaterialName = "Siyah Parlak Granit (Bien)", Description = "L�ks ve prestijli g�r�n�m.", ImageUrl = "https://images.unsplash.com/photo-1518599904199-0ca897819ddb?auto=format&fit=crop&w=400&q=80", PriceDifference = 5000 },

                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Duvar", MaterialName = "Beyaz Dalgal� �ini", Description = "Standart duvar d��emesi, ferah g�sterir.", ImageUrl = "https://images.unsplash.com/photo-1616486029423-aaa4789e8c9a?auto=format&fit=crop&w=400&q=80", PriceDifference = 0 },
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Duvar", MaterialName = "Antrasit Gri Dikd�rtgen Seramik", Description = "Koyu renk zeminlerle m�kemmel uyum.", ImageUrl = "https://images.unsplash.com/photo-1584622781564-1d987f7333c1?auto=format&fit=crop&w=400&q=80", PriceDifference = 0 },
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Banyo Duvar", MaterialName = "Alt�n Damarl� Calacatta Mermer", Description = "Premium duvar d��emesi.", ImageUrl = "https://images.unsplash.com/photo-1551000676-4767bd86fc38?auto=format&fit=crop&w=400&q=80", PriceDifference = 7500 },

                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Laminat Parke", MaterialName = "A��k Me�e (�amsan)", Description = "Standart salon ve oda parkesi.", ImageUrl = "https://images.unsplash.com/photo-1581858726788-75bc0f6a952d?auto=format&fit=crop&w=400&q=80", PriceDifference = 0 },
                    new ProjectMaterialCatalog { ConstructionProjectId = project.Id, Category = "Laminat Parke", MaterialName = "Koyu Ceviz Derzli (AGT)", Description = "Kal�n ve suya dayan�kl� l�ks parke.", ImageUrl = "https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=400&q=80", PriceDifference = 4000 }
                };
                context.ProjectMaterialCatalogs.AddRange(materials);
                await context.SaveChangesAsync();
            }
        }
    }
}






