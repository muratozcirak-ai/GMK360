import io
import re

filepath = r'GMK360.Data\Contexts\ApplicationDbContext.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

db_sets = """
        // YENI EKLENEN MODULLER (GARAGE, JURNAL, SANTIYE AVANS)
        public DbSet<GMK360.Core.Entities.Finance.ProjectCashRequest> ProjectCashRequests { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.CompanyVehicle> CompanyVehicles { get; set; }
        public DbSet<GMK360.Core.Entities.Logistics.VehicleAssignment> VehicleAssignments { get; set; }
        public DbSet<GMK360.Core.Entities.Construction.SiteDailyLog> SiteDailyLogs { get; set; }

        protected override void OnModelCreating
"""

content = re.sub(r'protected override void OnModelCreating', db_sets.strip('\r\n'), content)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected DbSets into ApplicationDbContext")
